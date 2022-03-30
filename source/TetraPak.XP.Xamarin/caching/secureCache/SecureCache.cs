using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Xamarin
{
    sealed class SecureCache : SimpleCache, ISecureCache
    {
        readonly SimpleCache _implementation;

        /// <inheritdoc />
        public override Task ConfigureAsync(string repository, ITimeLimitedRepositoryOptions options) 
            => _implementation.ConfigureAsync(repository, options);

        /// <inheritdoc />
        public override Task CreateAsync(object value,
            string key,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => _implementation.CreateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        /// <inheritdoc />
        public override Task<Outcome<T>> ReadAsync<T>(string key, string? repositoryName, CancellationToken? cancellationToken = null)
            => _implementation.ReadAsync<T>(key, repositoryName);

        /// <inheritdoc />
        public override Task UpdateAsync(object value,
            string key,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => _implementation.UpdateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        /// <inheritdoc />
        public override Task CreateOrUpdateAsync(object value,
            string key,
            string? repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => _implementation.CreateOrUpdateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        /// <inheritdoc />
        public override Task DeleteAsync(string key, string? repository = null) 
            => _implementation.DeleteAsync(key, repository);

        public SecureCache(ILog? log, params IITimeLimitedRepositoriesDelegate[] delegates) 
            : this(null!, log, delegates)
        {
        }
        
        public SecureCache(ITimeLimitedRepositories? implementation, ILog? log, params IITimeLimitedRepositoriesDelegate[] delegates) 
            : base(log, delegates)
        {
            _implementation = implementation as SimpleCache ?? new SimpleCache(log, delegates);
            if (!delegates.Any(i => i is XamarinSecureCacheDelegate))
            {
                _implementation.AddDelegates(new XamarinSecureCacheDelegate(CacheNames.SecureCache, log));
            }
        }

    }
}