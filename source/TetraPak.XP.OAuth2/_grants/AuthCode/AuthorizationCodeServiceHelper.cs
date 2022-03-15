using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.OAuth2.Refresh;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2.AuthCode
{
    public static class AuthorizationCodeServiceHelper
    {
        static bool s_isAuthCodeAdded;
        static readonly object s_syncRoot = new();
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds support for OAuth Authorization Code grant to the application and returns the <paramref name="collection"/>. 
        /// </summary>
        /// <param name="collection">
        ///   The extended service collection.
        /// </param>
        /// <param name="isSilentModeAllowed">
        ///   (optional; default=<c>true</c>)<br/>
        ///   When set the service collection is also configured for token caching and the OAuth2 Refresh Token grant. 
        /// </param>
        public static IServiceCollection UseTetraPakAuthorizationCodeGrant(
            this IServiceCollection collection, 
            bool isSilentModeAllowed = true)
        {
            lock (s_syncRoot)
            {
                if (s_isAuthCodeAdded)
                    return collection;

                s_isAuthCodeAdded = true;
            }
            
            collection.AddTetraPakConfiguration();
            collection.UseTetraPakHttpClientProvider();
            collection.AddSingleton<IAuthorizationCodeGrantService,TetraPakAuthorizationCodeGrantService>();
            if (isSilentModeAllowed)
            {
                collection.UseTetraPakRefreshTokenGrant();
            }
            return collection;
        }
    }
}