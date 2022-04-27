using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Mobile
{
    public static class XamarinCacheHelper
    {
        static readonly object s_syncRoot = new();
        static ITimeLimitedRepositories? s_cache;
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds a <see cref="ISecureCache"/> service to the service collection and then returns the service collection.
        /// </summary>
        /// <param name="services">
        ///   The service collection.
        /// </param>
        /// <param name="log">
        ///   A logging provider.
        /// </param>
        /// <returns>
        ///   The service collection.
        /// </returns>
        public static IServiceCollection AddSecureCache(
            this IServiceCollection services,
            ILog? log)
        {
            var secureCache = new SecureCache(log);
            ensureCache(secureCache, log);
            secureCache.AddSecureCacheSupport(log);
            services.AddSingleton<ISecureCache>(secureCache);
            return services;
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds support for secure (encrypted) caching to an existing cache and then returns the cache.
        /// </summary>
        /// <param name="cache">
        ///   An existing cache.  
        /// </param>
        /// <param name="log">
        ///   A logging provider.
        /// </param>
        /// <returns>
        ///   The <paramref name="cache"/> object.
        /// </returns>
        public static ITimeLimitedRepositories AddSecureCacheSupport(
            this ITimeLimitedRepositories cache,
            ILog? log)
        {
            cache.AddDelegates(new XamarinSecureCacheDelegate(CacheNames.SecureCache, log));
            return cache;
        }

        static ITimeLimitedRepositories ensureCache(ITimeLimitedRepositories? cache, ILog? log)
        {
            lock (s_syncRoot)
            {
                cache ??= s_cache ?? new SimpleCache(log);
                s_cache ??= cache;
                return cache;
            }
        }
    }
}