using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2.TokenExchange
{
    public static class TokenExchangeServiceHelper
    {
        static bool s_isTokenExchangeGrantAdded;
        static readonly object s_syncRoot = new();
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds support for OAuth2 Token Exchange grant to the application
        ///   and returns the <paramref name="collection"/>. 
        /// </summary>
        /// <param name="collection">
        ///   The extended service collection.
        /// </param>
        public static IServiceCollection AddTetraPakTokenExchangeGrant(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isTokenExchangeGrantAdded)
                    return collection;

                s_isTokenExchangeGrantAdded = true;
            }
            
            collection.AddTetraPakConfiguration();
            collection.AddTetraPakHttpClientProvider();
            collection.AddSingleton<ITokenExchangeGrantService,TetraPakTokenExchangeGrantService>();
            return collection;
        }
    }
}