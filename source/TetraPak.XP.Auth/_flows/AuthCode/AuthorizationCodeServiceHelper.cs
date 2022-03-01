using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.Auth.AuthCode
{
    public static class AuthorizationCodeServiceHelper
    {
        static bool s_isAuthCodeAdded;
        static readonly object s_syncRoot = new();

        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds support for OAuth Authorization Code grant to the application and returns the <paramref name="collection"/>. 
        /// </summary>
        public static IServiceCollection AddTetraPakDeviceCodeAuthentication(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isAuthCodeAdded)
                    return collection;

                s_isAuthCodeAdded = true;
            }
            
            collection.AddTetraPakConfiguration();
            collection.AddTetraPakHttpClientProvider();
            collection.AddSingleton<IAuthorizationCodeGrantService,TetraPakAuthorizationCodeGrantService>();
            return collection;
        }
    }
}