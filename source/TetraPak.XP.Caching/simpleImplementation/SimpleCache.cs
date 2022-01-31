using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging;

#nullable enable

namespace TetraPak.XP.Caching
{
    /// <summary>
    ///   Implements a very simple memory cache with automatic purging.
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class SimpleCache : ITimeLimitedRepositories
    {
        #if DEBUG
        static int s_debugObjectId;
        public int DebugObjectId { get; } = ++s_debugObjectId;
        #endif
        
        SimpleCacheConfig? _config;
        readonly Dictionary<string, DateTime> _lastPurge = new();
        TimeSpan? _defaultPurgeInterval;
        readonly List<IITimeLimitedRepositoriesDelegate> _delegates = new();
        
        [ThreadStatic]
        static DelegatesCollection s_currentDelegates;
        
        /// <summary>
        ///   (default=<c>false</c>)<br/>
        ///   Gets or sets a value to specify whether exceptions will be thrown when attempting to perform
        ///   potentially invalid operations, such as removing or updating a "dead" item.     
        /// </summary>
        public bool IsStrict { get; set; } = false;

        /// <summary>
        ///   Gets a logging provider.
        /// </summary>
        protected ILog? Log { get; }

        /// <inheritdoc />
        public string? DefaultRepository { get; set; }

        /// <inheritdoc />
        public string? DefaultKey { get; set; }

        /// <inheritdoc />
        public TimeSpan DefaultLifeSpan { get; set; } = TimeSpan.FromMinutes(1);
        
        /// <inheritdoc />
        public TimeSpan DefaultExtendedLifeSpan { get; set; } = TimeSpan.Zero;

        /// <inheritdoc />
        public TimeSpan DefaultMaxLifeSpan { get; set; } = TimeSpan.Zero;
        
        /// <inheritdoc />
        public TimeSpan DefaultAdjustedLifeSpan { get; set; } = TimeSpan.Zero;

        ITimeLimitedRepositoryOptions DefaultOptions => SimpleTimeLimitedRepositoryOptions.AsDefault(this);
        
        /// <summary>
        ///   Gets or sets an interval between automatic purging processes, to be used for repositories that
        ///   supports this feature and that isn't configured with a custom purging interval.
        /// </summary>
        /// <remarks>
        ///   By setting this value to anything lower than <see cref="TimeSpan.MaxValue"/> the
        ///   <see cref="SimpleCache"/> will request purging from all repositories, to remove all entries regularly
        ///   to avoid resources leaks.  
        /// </remarks>
        public TimeSpan DefaultPurgeInterval
        {
            get => _defaultPurgeInterval ?? SimpleTimeLimitedRepositoryOptions.DefaultPurgeInterval;
            set => _defaultPurgeInterval = value;
        }

        /// <inheritdoc />
        public bool IsTypeStrict { get; set; } = true;

        /// <inheritdoc />
        public virtual async Task CreateAsync(object value,
            string key,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
        {
            var firstDelegate = _delegates.FirstOrDefault();
            if (firstDelegate is null)
                return;
            
            spawnTimeUtc ??= DateTime.UtcNow;
            var resolvedRepository = repository.EnsureAssigned(nameof(repository), DefaultRepository);
            var path = new RepositoryPath(resolvedRepository, key);
            var entry = new SimpleCacheEntry(this, path!, value, spawnTimeUtc.Value, customLifeSpan);

            setCurrentDelegates();
            foreach (var @delegate in s_currentDelegates)
            {
                if (! await @delegate.CreateAsync(entry, IsStrict))
                    continue;
                
                purgeIfNeeded(repository, @delegate);
                break;
            }
        }

        /// <inheritdoc />
        public virtual async Task<Outcome<T>> ReadAsync<T>(string key, string? repository)
        {
            var path = makeItemPath(key, repository);
            ITimeLimitedRepositoryEntry? entry = null;
            foreach (var @delegate in _delegates)
            {
                if (entry is null)
                {
                    var outcome = await @delegate.ReadRawEntryAsync(path);
                    if (!outcome)
                        continue;

                    entry = outcome.Value!;
                    if (entry is SimpleCacheEntry simpleCacheEntry)
                    {
                        simpleCacheEntry.SourceDelegate = @delegate;
                    }
                    purgeIfNeeded(repository, @delegate);
                }

                var validatedOutcome = await @delegate.GetValidItemAsync<T>(entry);
                if (!validatedOutcome)
                    continue;
                
                purgeIfNeeded(repository, @delegate);
                return Outcome<T>.Success(validatedOutcome.Value.Value);
            }
            
            return Outcome<T>.Fail(new Exception($"Failed to read {path}"));
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(
            object value,
            string key,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
        {
            var path = new RepositoryPath(repository, key);
            var entry = new SimpleCacheEntry(this, path!, value, spawnTimeUtc ?? DateTime.UtcNow, customLifeSpan);
            foreach (var @delegate in _delegates)
            {
                if (!await @delegate.UpdateAsync(entry, IsStrict))
                    continue;
                
                purgeIfNeeded(repository, @delegate);
                break;
            }
        }

        /// <inheritdoc />
        public virtual async Task CreateOrUpdateAsync(object value,
            string key,
            string? repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
        {
            var path = new RepositoryPath(repository, key);
            var entry = new SimpleCacheEntry(this, path!, value, spawnTimeUtc ?? DateTime.UtcNow, customLifeSpan);
            foreach (var @delegate in _delegates)
            {
                if (!await @delegate.CreateOrUpdateAsync(entry, IsStrict))
                    continue;
                
                purgeIfNeeded(repository, @delegate);
                break;
            }
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(string key, string? repository = null)
        {
            var path = new RepositoryPath(repository, key);
            foreach (var @delegate in _delegates)
            {
                if (!await @delegate.DeleteAsync(path, IsStrict))
                    continue;
                
                purgeIfNeeded(repository, @delegate);
                break;
            }
        }

        /// <inheritdoc />
        public TimeSpan GetLifeSpan(string repository)
        {
            return _config?.GetRepositoryOptions(repository)?.LifeSpan ?? DefaultLifeSpan;
        }

        /// <inheritdoc />
        public TimeSpan GetMaxLifeSpan(string repository)
        {
            return _config?.GetRepositoryOptions(repository)?.MaxLifeSpan ?? DefaultMaxLifeSpan;
        }

        /// <inheritdoc />
        public TimeSpan GetExtendedLifeSpan(string repository)
        {
            return _config?.GetRepositoryOptions(repository)?.ExtendedLifeSpan ?? DefaultLifeSpan;
        }

        /// <inheritdoc />
        public TimeSpan GetAdjustedLifeSpan(string repository)
        {
            return _config?.GetRepositoryOptions(repository)?.AdjustedLifeSpan ?? DefaultAdjustedLifeSpan;
        }

        /// <inheritdoc />
        public virtual Task ConfigureAsync(string repository, ITimeLimitedRepositoryOptions options)
        {
            if (options is SimpleTimeLimitedRepositoryOptions config)
            {
                config.SetCache(this);
            }
            _config!.Configure(repository, options);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<ITimeLimitedRepositoryOptions> GetRepositoryOptionsAsync(string repository, bool useDefault = true)
        {
            var options = _config!.GetRepositoryOptions(repository);
            return Task.FromResult(options ?? (useDefault
                ? DefaultOptions
                : null!));
        }
        
        /// <inheritdoc />
        public void AddDelegates(params IITimeLimitedRepositoriesDelegate[] delegates)
        {
            IITimeLimitedRepositoriesDelegate? next = null;
            for (var i = delegates.Length-1; i >= 0; i--)
            {
                var @delegate = delegates[i];
                _delegates.Insert(0, @delegate);
                (@delegate as SimpleCacheDelegate)?.SetNextDelegate(next);
                next = @delegate;
                (@delegate as SimpleCacheDelegate)?.Attach(this);
            }
        }

        /// <inheritdoc />
        public bool ContainsDelegate<T>() where T : IITimeLimitedRepositoriesDelegate 
            => _delegates.Any(i => i is T);

        /// <inheritdoc />
        public int InsertDelegateBefore<T>(IITimeLimitedRepositoriesDelegate @delegate)
            where T : IITimeLimitedRepositoriesDelegate
            => InsertDelegateBefore(@delegate, del => del is T);

        /// <inheritdoc />
        public int InsertDelegateBefore(
            IITimeLimitedRepositoriesDelegate @delegate, 
            Func<IITimeLimitedRepositoriesDelegate, bool> criteria)
        {
            lock (_delegates)
            {
                var array = _delegates.ToArray();
                IITimeLimitedRepositoriesDelegate? previous = null; 
                for (var index = 0; index < array.Length; index++)
                {
                    var next = array[index];
                    if (!criteria(next))
                    {
                        previous = next;
                        continue;
                    }
                    
                    (previous as SimpleCacheDelegate)?.SetNextDelegate(next);
                    if (index < array.Length - 1)
                    {
                        (@delegate as SimpleCacheDelegate)?.SetNextDelegate(array[array.Length-1]);
                    }

                    _delegates.Insert(index, @delegate);
                    (@delegate as SimpleCacheDelegate)?.Attach(this);
                    return index;
                }
            }
            
            return -1;
        }

        /// <summary>
        ///   This method gets called from a background thread to allow selecting entries to be removed.
        ///   The default implementation simply returns a collection of entries that are "dead"
        ///   (the <see cref="ITimeLimitedRepositoryEntry.IsLive"/> returns <c>false</c>).
        /// </summary>
        /// <param name="entries">
        ///   A collection of entries that should be examined. 
        /// </param>
        /// <returns>
        ///   A collection of entries that should be removed.
        /// </returns>
        protected virtual Task<ITimeLimitedRepositoryEntry[]> OnGetDeadEntries(
            IEnumerable<ITimeLimitedRepositoryEntry> entries)
        {
            return Task.FromResult(entries.Where(entry => !entry.IsLive()).ToArray());
        }

        /// <summary>
        ///   Invoked internally to construct a path (key) for a cached item in a targeted repository.
        /// </summary>
        /// <param name="key">
        ///     Identifies the cached item.
        /// </param>
        /// <param name="repository">
        ///     The targeted repository.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/>.
        /// </returns>   
        protected virtual DynamicPath OnMakeItemPath(string key, string repository) 
        => new RepositoryPath(repository, key).StringValue!;
        
        DynamicPath makeItemPath(string key, string? repository) => OnMakeItemPath(key, repository);

        /// <summary>
        ///   (fluent api)<br/>
        ///   Applies cache configuration and returns <c>this</c> instance. 
        /// </summary>
        public SimpleCache WithConfiguration(SimpleCacheConfig config)
        {
            _config = config;
            return this;
        }
        
        void purgeIfNeeded(string repository, IITimeLimitedRepositoriesDelegate @delegate)
        {
            var now = DateTime.Now;

            var defaultInterval = SimpleTimeLimitedRepositoryOptions.DefaultPurgeInterval;
            var interval = _config?.GetRepositoryPurgeInterval(repository, defaultInterval) ?? defaultInterval;
            var nextPurgeAt = getLastPurge().Add(interval); 
            if (nextPurgeAt >= now)
            {
                @delegate.PurgeNowAsync();
                _lastPurge[repository] = now;
            }
            
            DateTime getLastPurge()
            {
                if (_lastPurge.TryGetValue(repository, out var purgeTime))
                    return purgeTime;
                
                _lastPurge.Add(repository, now);
                return now;
            }
        }
        
        public static async Task<Outcome> NextCreateAsync(
            SimpleCacheDelegate caller, 
            ITimeLimitedRepositoryEntry entry,
            bool strict, Func<IITimeLimitedRepositoriesDelegate, bool>? filter)
        {
            s_currentDelegates.SkipTo(filter);
            return caller.NextDelegate is null
                ? Outcome.Fail(new Exception("No delegate could handle the creation"))
                : await caller.NextDelegate.CreateAsync(entry, strict);
        }

        public static async Task<Outcome<ITimeLimitedRepositoryEntry>> NextReadRawEntryAsync(SimpleCacheDelegate caller,
            DynamicPath path, Func<IITimeLimitedRepositoriesDelegate, bool> filter)
        {
            s_currentDelegates.End();
            return caller.NextDelegate is null
                ? Outcome<ITimeLimitedRepositoryEntry>.Fail(new Exception("No delegate could handle the raw read"))
                : await caller.NextDelegate.ReadRawEntryAsync(path);
        }

        public static async Task<Outcome> NextUpdateAsync(SimpleCacheDelegate caller, ITimeLimitedRepositoryEntry entry,
            bool strict, Func<IITimeLimitedRepositoriesDelegate, bool> filter)
        {
            s_currentDelegates.End();
            return caller.NextDelegate is null
                ? Outcome<ITimeLimitedRepositoryEntry>.Fail(new Exception("No delegate could handle the update"))
                : await caller.NextDelegate.UpdateAsync(entry, strict);
        }

        public static async Task<Outcome> NextCreateOrUpdateAsync(SimpleCacheDelegate caller,
            ITimeLimitedRepositoryEntry entry, bool strict, Func<IITimeLimitedRepositoriesDelegate, bool> filter)
        {
            s_currentDelegates.End();
            return caller.NextDelegate is null
                ? Outcome<ITimeLimitedRepositoryEntry>.Fail(new Exception("No delegate could handle the create/update"))
                : await caller.NextDelegate.CreateOrUpdateAsync(entry, strict);
        }

        public static async Task<Outcome> NextDeleteAsync(SimpleCacheDelegate caller, DynamicPath path, bool strict,
            Func<IITimeLimitedRepositoriesDelegate, bool> filter)
        {
            s_currentDelegates.End();
            return caller.NextDelegate is null
                ? Outcome<ITimeLimitedRepositoryEntry>.Fail(new Exception("No delegate could handle the delete"))
                : await caller.NextDelegate.DeleteAsync(path, strict);
        }

        void setCurrentDelegates() => s_currentDelegates = new DelegatesCollection(_delegates);

        /// <summary>
        ///   Initializes the <see cref="SimpleCache"/>.
        /// </summary>
        /// <param name="log">
        ///   A logger provider to be used for internal logging purposes.
        /// </param>
        /// <param name="delegates">
        ///   (optional)<br/>
        ///   Delegates allowing caching logics customization.
        /// </param>
        public SimpleCache(ILog? log, params IITimeLimitedRepositoriesDelegate[] delegates)
        {
            Log = log;
            if (!delegates.Any())
            {
                AddDelegates(new SimpleCacheDelegate(log));
            }
        }

        static SimpleCache() => s_currentDelegates = DelegatesCollection.Empty;
    }
}