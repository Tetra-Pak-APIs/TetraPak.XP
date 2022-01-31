using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Caching;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth
{
    public static class TokenCacheHelper
    {
        /// <summary>
        ///   (fluent api)<br/>
        ///   Provides an <see cref="ITokenCache"/> implementation. and returns the <see cref="IServiceCollection"/><br/>
        ///   CAUTION! This requires that a <see cref="ISecureCache"/> service is also provided,
        ///   or the service locator will fail and throw an exception in runtime.
        /// </summary>
        /// <param name="services">
        ///   The service collection. 
        /// </param>
        /// <returns>
        ///   The <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddTokenCache(this IServiceCollection services)
        {
            services.AddSingleton<ITokenCache>(p => new TokenCache(p));
            return services;
        }

        // /// <summary>
        // ///   (fluent api)<br/>
        // ///   Adds a token caching mechanism to a provided <see cref="ISecureCache"/> and returns it.  obsolete
        // /// </summary>
        // /// <param name="secureCache">
        // ///   The provided <see cref="ISecureCache"/> service.
        // /// </param>
        // /// <param name="log">
        // ///   (optional)<br/>
        // ///   A logger provider.
        // /// </param>
        // ///  <returns>
        // ///   The <paramref name="secureCache"/> instance.
        // /// </returns>
        // public static ITimeLimitedRepositories AddTokenCacheSupport(this ISecureCache secureCache, ILog? log)
        // {
        //     secureCache.InsertDelegateBefore<ISecureCacheDelegate>(new TokenCacheDelegate(log));
        //     return secureCache;
        // }
    }
}