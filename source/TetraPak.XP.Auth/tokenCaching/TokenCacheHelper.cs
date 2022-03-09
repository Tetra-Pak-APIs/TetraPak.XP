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
    }
}