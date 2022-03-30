using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Caching.Abstractions;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   
    /// </summary>
    public static class TokenCacheHelper
    {
        /// <summary>
        ///   (fluent api)<br/>
        ///   Provides a simple memory-based <see cref="ITokenCache"/> implementation and returns the
        ///   <see cref="IServiceCollection"/><br/>.
        ///   CAUTION! This requires that a <see cref="ISecureCache"/> service is also provided,
        ///   or the service locator will fail and throw an exception in runtime.
        /// </summary>
        /// <param name="collection">
        ///   The service collection. 
        /// </param>
        /// <returns>
        ///   The <paramref name="collection"/>.
        /// </returns>
        public static IServiceCollection UseTokenCache(this IServiceCollection collection)
        {
            collection.AddSingleton<ITokenCache>(p => new TokenCache(p));
            return collection;
        }
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Provides a <see cref="ITokenCache"/> implementation and returns the
        ///   <see cref="IServiceCollection"/><br/>.
        /// </summary>
        /// <param name="collection">
        ///   The service collection. 
        /// </param>
        /// <returns>
        ///   The <paramref name="collection"/>.
        /// </returns>
        public static IServiceCollection UseTokenCache<T>(this IServiceCollection collection)
        where T : class, ITokenCache
        {
            collection.AddSingleton<ITokenCache,T>();
            return collection;
        }
    }
}