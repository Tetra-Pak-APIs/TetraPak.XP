using System;
using System.Linq;
using System.Threading.Tasks;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Caching
{
    public class SimpleFileCache : SimpleCache, IFileCache
    {
        protected SimpleCache Implementation { get; }

        public override Task CreateAsync(object value,
            string key,
            string? repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => Implementation.CreateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        public override Task<Outcome<T>> ReadAsync<T>(string key, string? repository)
            => Implementation.ReadAsync<T>(key, repository);

        public override Task UpdateAsync(object value,
            string key,
            string? repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => Implementation.UpdateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        public override Task CreateOrUpdateAsync(object value,
            string key,
            string? repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => Implementation.CreateOrUpdateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        public override Task DeleteAsync(string key, string? repository)
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
            if (!(delegates?.Any(i => i is FileCacheDelegate) ?? true))
            {
                Implementation.AddDelegates(new FileCacheDelegate(CacheNames.FileCache, options, log));
            }
        }
    }
}