using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Caching
{
    public class SimpleCacheDelegate : IITimeLimitedRepositoriesDelegate
    {
        readonly IDictionary<string, SimpleCacheEntry> _values;
        TaskCompletionSource<bool>? _purgingTcs;
        DateTime _lastPurge;
        readonly ILog? _log;
        string? _targetRepository;

        /// <summary>
        ///   Gets or sets an (optional) target repository name, allowing simple filtering for requests. 
        /// </summary>
        /// <seealso cref="WithTargetRepository"/>
        public string? TargetRepository
        {
            get => _targetRepository;
            set => _targetRepository = value is null || value.IsAssigned() ? value : throw new ArgumentNullException(nameof(value));
        }

        internal IITimeLimitedRepositoriesDelegate? NextDelegate { get; private set; }

        public TimeSpan AutoPurgeInterval { get; set; } = TimeSpan.FromMinutes(5);

        internal void SetNextDelegate(IITimeLimitedRepositoriesDelegate? @delegate) => NextDelegate = @delegate;
        
        public virtual Task<Outcome> CreateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            lock (_values)
            {
                var path = (RepositoryPath) entry.Path;
                if (!_values.TryGetValue(path.StringValue, out var existingEntry))
                {
                    var setEntry = entry is SimpleCacheEntry simpleCacheEntry
                        ? simpleCacheEntry
                        : new SimpleCacheEntry(entry.Repositories, path, entry.GetValue(), DateTime.UtcNow);
                    _values.Add(path, setEntry); 
                }
                else
                {
                    if (entry.IsLive())
                        return strict
                            ? throw new IdentityConflictException(
                                nameof(path),
                                $"Cannot add new cached value '{path}'. Value is already cached")
                            : Task.FromResult(Outcome.Fail(new Exception($"Value not found: {path}")));

                    var customLifeSpan = entry is SimpleCacheEntry simpleCacheEntry
                        ? simpleCacheEntry.CustomLifeSpan
                        : null;
                    existingEntry.UpdateValue(entry.GetValue(), entry.SpawnTimeUtc, customLifeSpan);
                }
                return Task.FromResult(Outcome.Success());
            }
        }

        public virtual Task<Outcome<ITimeLimitedRepositoryEntry>> ReadRawEntryAsync(DynamicPath path)
        {
            PurgeNowAsync();
            lock (_values)
            {
                return _values.TryGetValue(path, out var entry)
                    ? Task.FromResult(Outcome<ITimeLimitedRepositoryEntry>.Success(entry))
                    : Task.FromResult(Outcome<ITimeLimitedRepositoryEntry>.Fail(
                        new ArgumentOutOfRangeException(nameof(path), $"Unknown value: {path}")));
            }
        }

        public virtual Task<Outcome> UpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            var path = entry.Path;
            lock (_values)
            {
                var updatingEntry = (SimpleCacheEntry)entry;
                if (!_values.TryGetValue(entry.Path, out var rawEntry))
                {
                    var exception = new ArgumentOutOfRangeException(
                        nameof(entry.Path),
                        $"Cannot update cached value '{path}'. Value does not exist");
                    return strict
                        ? throw exception
                        : Task.FromResult(Outcome.Fail(exception));
                }

                rawEntry.UpdateValue(entry.GetValue(), updatingEntry.SpawnTimeUtc, updatingEntry.CustomLifeSpan);
                return Task.FromResult(Outcome.Success());
            }
        }

        public virtual Task<Outcome> CreateOrUpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            PurgeNowAsync();
            var path = (RepositoryPath) entry.Path;

            // var simpleCacheEntry = new SimpleCacheEntry(entry.Repositories, path.Repository, path.Repository, value, spawnTimeUtc.Value, customLifeSpan);
            lock (_values)
            {
                var simpleCacheEntry = entry as SimpleCacheEntry; 
                var spawnTimeUTC = simpleCacheEntry?.SpawnTimeUtc ?? DateTime.UtcNow;
                var customLifeSpan = simpleCacheEntry?.CustomLifeSpan ?? null;                    
                var newEntry = new SimpleCacheEntry(
                    entry.Repositories, 
                    path,
                    entry.GetValue(), 
                    spawnTimeUTC,
                    customLifeSpan);

                if (!_values.TryGetValue(path, out var existingEntry))
                {
                    _values.Add(path, newEntry);                   
                    return Task.FromResult(Outcome.Success());
                }

                if (!entry.IsLive())
                {
                    _log.Trace($"Updating dead cached value '{path}'. Removing and re-adding it");
                    _values.Remove(path);
                    _values.Add(path, newEntry);
                }
                existingEntry.UpdateValue(entry.GetValue(), spawnTimeUTC, customLifeSpan);
                return Task.FromResult(Outcome.Success());
            }
        }

        public Task<Outcome> DeleteAsync(DynamicPath path, bool strict)
        {
            lock (_values)
            {
                if (!_values.TryGetValue(path.StringValue, out _))
                {
                    return strict
                        ? throw new ArgumentOutOfRangeException(nameof(path), $"Unknown value: {path}")
                        : Task.FromResult(Outcome.Fail(new Exception($"Value not found: {path}")));
                }

                _values.Remove(path.StringValue);
            }
            return Task.FromResult(Outcome.Success());
        }

        protected Task<Outcome> DelegateCreateAsync(
            ITimeLimitedRepositoryEntry entry, 
            bool strict, 
            Func<IITimeLimitedRepositoriesDelegate,bool>? filter = null) 
            => SimpleCache.NextCreateAsync(this, entry, strict, filter);

        protected Task<Outcome<ITimeLimitedRepositoryEntry>> DelegateReadRawEntryAsync(
            DynamicPath path, 
            Func<IITimeLimitedRepositoriesDelegate,bool>? filter = null) 
            => SimpleCache.NextReadRawEntryAsync(this, path, filter);

        protected Task<Outcome> DelegateUpdateAsync(
            ITimeLimitedRepositoryEntry entry, 
            bool strict, 
            Func<IITimeLimitedRepositoriesDelegate,bool>? filter = null) 
            => SimpleCache.NextUpdateAsync(this, entry, strict, filter);

        protected Task<Outcome> DelegateCreateOrUpdateAsync(
            ITimeLimitedRepositoryEntry entry, 
            bool strict, 
            Func<IITimeLimitedRepositoriesDelegate,bool>? filter = null) 
            => SimpleCache.NextCreateOrUpdateAsync(this, entry, strict, filter);

        protected Task<Outcome> DelegateDeleteAsync(
            DynamicPath path,
            bool strict, 
            Func<IITimeLimitedRepositoriesDelegate,bool>? filter = null)
            => SimpleCache.NextDeleteAsync(this, path, strict, filter);

        public virtual Task<Outcome<CachedItem<T>>> GetValidItemAsync<T>(ITimeLimitedRepositoryEntry entry)
        {
            if (entry.Repositories is not SimpleCache)
                return Task.FromResult(
                    Outcome<CachedItem<T>>.Fail(
                        new InvalidOperationException($"Cache delegate expected repository of type {typeof(SimpleCache)}")));
            
            var path = (RepositoryPath)entry.Path;

            var valueType = entry.GetValueType();
            if (!typeof(T).IsAssignableFrom(valueType))
                return Task.FromResult(Outcome<CachedItem<T>>.Fail(
                    new InvalidCastException($"Cannot cast value of type {valueType} to {typeof(T)}")));
            
            if (!entry.IsLive(out var expireTimeUtc))
                return Task.FromResult(
                    Outcome<CachedItem<T>>.Fail(
                        new ArgumentOutOfRangeException($"Cached value is not available: {entry.Path}")));

            var value = (T)entry.GetValue();
            var extendedLifSpan = entry.Repositories.GetExtendedLifeSpan(path.Repository);
            if (extendedLifSpan != TimeSpan.Zero)
            {
                entry.ExtendLifeSpan();
            }
            var cachedItem = new CachedItem<T>(entry.Path, value, expireTimeUtc);
            return Task.FromResult(Outcome<CachedItem<T>>.Success(cachedItem));
        }

        public Task PurgeNowAsync()
        {
            lock (_values)
            {
                if (_purgingTcs is { })
                    return _purgingTcs.Task;
                
                if (AutoPurgeInterval == TimeSpan.MaxValue || _lastPurge.Add(AutoPurgeInterval) < DateTime.Now)
                    return Task.CompletedTask;
            }
            
            _log.Debug($"Commences automatic purging from {this} ...");
            
            _purgingTcs = new TaskCompletionSource<bool>();
            Task.Run(async () =>
            {
                var allEntries = _values.Values.Cast<ITimeLimitedRepositoryEntry>().ToArray(); 
                var purgedEntries = await getDeadEntries(allEntries);
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < purgedEntries.Length; i++)
                {
                    var e = purgedEntries[i];
                    await DeleteAsync((RepositoryPath) e.Path, true);
                }
                _lastPurge = DateTime.Now;
                _purgingTcs.SetResult(true);
                _purgingTcs = null;
                _log.Debug($"Automatic purging from {this} is DONE");
            });
            return _purgingTcs.Task;
            
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Assigns the <see cref="TargetRepository"/> and returns <c>this</c>.
        /// </summary>
        public SimpleCacheDelegate WithTargetRepository(string targetRepository)
        {
            TargetRepository = targetRepository;
            return this;
        }

        /// <summary>
        ///   Examines a <see cref="RepositoryPath"/> and returns a value indicating that it represents
        ///   the targeted repository. Please note that if <see cref="TargetRepository"/> is unassigned (<c>null</c>)
        ///   the <paramref name="path"/> is considered a targeted repository. 
        /// </summary>
        /// <param name="path">
        ///   A <see cref="RepositoryPath"/> to be examined.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the path's repository element matches the <see cref="TargetRepository"/> value
        ///   or the <see cref="TargetRepository"/> value is <c>null</c>; otherwise <c>false</c>.
        /// </returns>
        protected bool IsTargetRepository(RepositoryPath path) => TargetRepository is null || IsTargetRepository(path.Repository);

        /// <summary>
        ///   Examines a repository name and returns a value indicating that it represents
        ///   the targeted repository. Please note that if <see cref="TargetRepository"/> is unassigned (<c>null</c>)
        ///   the <paramref name="repository"/> is considered a targeted repository. 
        /// </summary>
        /// <param name="repository">
        ///   The repository name to be examined.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="repository"/> matches the <see cref="TargetRepository"/> value
        ///   or the <see cref="TargetRepository"/> value is <c>null</c>; otherwise <c>false</c>.
        /// </returns>
        protected bool IsTargetRepository(string repository) =>  repository == TargetRepository;

        static Task<ITimeLimitedRepositoryEntry[]> getDeadEntries(IEnumerable<ITimeLimitedRepositoryEntry> entries)
        {
            return Task.FromResult(entries.Where(entry => !entry.IsLive()).ToArray());
        }

        internal void Attach(SimpleCache cache) => OnAttachedToCache(cache);

        /// <summary>
        ///   (virtual method; to be overriden; no base implementation)<br/>
        ///   Invoked when delegate is attached to a <see cref="SimpleCache"/> object. 
        /// </summary>
        // ReSharper disable once UnusedParameter.Global
        protected virtual void OnAttachedToCache(SimpleCache cache)
        {
            // to be overridden
        }

        public SimpleCacheDelegate(ILog? log)
        {
            _values = new Dictionary<string, SimpleCacheEntry>();
            _log = log;
        }
    }
}