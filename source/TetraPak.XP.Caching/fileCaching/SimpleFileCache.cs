using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Caching
{
    public class SimpleFileCache : SimpleCache, IFileCache
    {
        protected SimpleCache Implementation { get; }

        /// <inheritdoc />
        public override Task CreateAsync(object value,
            string key,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => Implementation.CreateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        /// <inheritdoc />
        public override Task<Outcome<T>> ReadAsync<T>(
            string key, 
            string? repository, 
            CancellationToken? cancellationToken = null)
            => Implementation.ReadAsync<T>(key, repository);

        /// <inheritdoc />
        public override Task UpdateAsync(object value,
            string key,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => Implementation.UpdateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        /// <inheritdoc />
        public override Task CreateOrUpdateAsync(object value,
            string key,
            string? repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => Implementation.CreateOrUpdateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        /// <inheritdoc />
        public override Task DeleteAsync(string key, string? repository = null)
            => Implementation.DeleteAsync(key, repository);

        public SimpleFileCache(
            ILog? log,
            IFileSystem fileSystem,
            FileCacheOptions? options,
            params IITimeLimitedRepositoriesDelegate[] delegates)
        : this(null!, log, fileSystem, options, delegates)
        {
        }

        public SimpleFileCache(
            SimpleCache? implementation,
            ILog? log,
            IFileSystem fileSystem,
            FileCacheOptions? options,
            params IITimeLimitedRepositoriesDelegate[] delegates) 
        : base(log, delegates)
        {
            Implementation = implementation ?? this;
            options ??= FileCacheOptions.Default(fileSystem);
            if (!delegates.Any(i => i is FileCacheDelegate))
            {
                Implementation.AddDelegates(new FileCacheDelegate(CacheNames.FileCache, options, log));
            }
        }
    }
}