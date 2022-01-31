using System;
using System.Linq;
using System.Threading.Tasks;
using TetraPak.XP.Caching;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Xamarin.Caching
{
    class XamarinSecureCache : SimpleCache, ISecureCache
    {
        readonly SimpleCache _implementation;

        public override Task ConfigureAsync(string repository, ITimeLimitedRepositoryOptions options) 
            => _implementation.ConfigureAsync(repository, options);

        public override Task CreateAsync(
            string repository,
            string key,
            object value,
            TimeSpan? customLifeSpan = null, 
            DateTime? spawnTimeUtc = null)
            => _implementation.CreateAsync(repository, key, value, customLifeSpan, spawnTimeUtc);

        public override Task<Outcome<T>> ReadAsync<T>(string repository, string key)
            => _implementation.ReadAsync<T>(repository, key);

        public override Task UpdateAsync(
            string repository,
            string key,
            object value,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => _implementation.UpdateAsync(repository, key, value, customLifeSpan, spawnTimeUtc);

        public override Task CreateOrUpdateAsync(
            string repository, 
            string key, 
            object value, 
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
            => _implementation.CreateOrUpdateAsync(repository, key, value, customLifeSpan, spawnTimeUtc);

        public override Task DeleteAsync(string repository, string key) 
            => _implementation.DeleteAsync(repository, key);

        public XamarinSecureCache(ILog? log, params IITimeLimitedRepositoriesDelegate[] delegates) 
            : this(null!, log, delegates)
        {
        }
        
        public XamarinSecureCache(ITimeLimitedRepositories? implementation, ILog? log, params IITimeLimitedRepositoriesDelegate[] delegates) 
            : base(log, delegates)
        {
            _implementation = implementation as SimpleCache ?? new SimpleCache(log, delegates);
            if (!(delegates?.Any(i => i is SecureCacheDelegate) ?? true))
            {
                _implementation.AddDelegates(new SecureCacheDelegate(Caches.SecureCache, log));
            }
        }

    }
}