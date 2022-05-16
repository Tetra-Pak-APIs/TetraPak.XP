using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Auth.Abstractions
{
    sealed class TokenCache : SimpleCache, ITokenCache
    {
        const string DefaultTokenCacheRepository = "securityTokens"; // note example: secureStore://securityTokens/SEREMRATTJ 

        readonly ISecureCache _secureCache;

        public override Task ConfigureAsync(string repository, ITimeLimitedRepositoryOptions options)
            => _secureCache.ConfigureAsync(repository, options);

        public override Task CreateAsync(
            object value,
            string key,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
        {
            key = ensureTokensSubRepository(key);
            repository ??= DefaultTokenCacheRepository;
            return _secureCache.CreateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);
        }

        public override Task<Outcome<T>> ReadAsync<T>(string key, string? repository, CancellationToken? cancellationToken = null)
        {
            key = ensureTokensSubRepository(key);
            repository ??= DefaultTokenCacheRepository;
            return _secureCache.ReadAsync<T>(key, repository, cancellationToken);
        }

        public override Task UpdateAsync(
            object value,
            string key,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
        {
            key = ensureTokensSubRepository(key);
            return _secureCache.UpdateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);
        }

        public override Task CreateOrUpdateAsync(object value,
            string key,
            string? repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
        {
            key = ensureTokensSubRepository(key);
            return _secureCache.CreateOrUpdateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);
        }

        public override Task DeleteAsync(string key, string? repository = null)
        {
            key = ensureTokensSubRepository(key);
            return _secureCache.DeleteAsync(key, repository);
        }

        static string ensureTokensSubRepository(string key)
        {
            var path = (DynamicPath) key;
            return path.Count switch
            {
                0 => DefaultTokenCacheRepository,
                1 => path == DefaultTokenCacheRepository ? key : new DynamicPath(DefaultTokenCacheRepository, key),
                _ => path[0] != DefaultTokenCacheRepository
                    ? new DynamicPath(path.Items.WithInserted(0, DefaultTokenCacheRepository).ToArray())
                    : key
            };
        }

        TokenCache(ISecureCache secureCache, ILog? log)
        : base(log)
        {
            _secureCache = secureCache.ThrowIfNull(nameof(secureCache));
            if (!secureCache.ContainsDelegate<ISecureCacheDelegate>()) 
                throw new Exception(
                    "Could not activate token cache because provided secure cache was not correctly provided");
        }

        internal TokenCache(IServiceProvider provider) 
        : this(provider.GetRequiredService<ISecureCache>(), provider.GetService<ILog>())
        {
        }
    }
}