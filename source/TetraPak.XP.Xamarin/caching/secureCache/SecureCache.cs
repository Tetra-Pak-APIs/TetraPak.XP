using System;
using System.Linq;
using System.Threading.Tasks;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Xamarin
{
    class SecureCache : SimpleCache, ISecureCache
    {
        readonly SimpleCache _implementation;

        public override Task ConfigureAsync(string repository, ITimeLimitedRepositoryOptions options) 
            => _implementation.ConfigureAsync(repository, options);

        public override Task CreateAsync(object value,
            string key,
            string? repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => _implementation.CreateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        public override Task<Outcome<T>> ReadAsync<T>(string key, string? repositoryName)
            => _implementation.ReadAsync<T>(key, repositoryName);

        public override Task UpdateAsync(object value,
            string key,
            string? repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => _implementation.UpdateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        public override Task CreateOrUpdateAsync(object value,
            string key,
            string? repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => _implementation.CreateOrUpdateAsync(value, key, repository, customLifeSpan, spawnTimeUtc);

        public override Task DeleteAsync(string key, string? repository) 
            => _implementation.DeleteAsync(key, repository);

        public SecureCache(ILog? log, params IITimeLimitedRepositoriesDelegate[] delegates) 
            : this(null!, log, delegates)
        {
        }
        
        public SecureCache(ITimeLimitedRepositories? implementation, ILog? log, params IITimeLimitedRepositoriesDelegate[] delegates) 
            : base(log, delegates)
        {
            _implementation = implementation as SimpleCache ?? new SimpleCache(log, delegates);
            if (!(delegates?.Any(i => i is XamarinSecureCacheDelegate) ?? true))
            {
                _implementation.AddDelegates(new XamarinSecureCacheDelegate(CacheNames.SecureCache, log));
            }
        }

    }
}