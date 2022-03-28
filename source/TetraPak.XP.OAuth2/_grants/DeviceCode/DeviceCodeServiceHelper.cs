using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.OAuth2.Refresh;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2.DeviceCode
{
    /// <summary>
    ///   Helps with adding support for the OAuth2 Device Code grant type
    ///   (see <see cref="AddTetraPakDeviceCodeGrant"/>). 
    /// </summary>
    public static class DeviceCodeServiceHelper
    {
        static bool s_isClientCredentialsAdded;
        static readonly object s_syncRoot = new();

        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds support for OAuth Device Code grant to the application and returns the <paramref name="collection"/>. 
        /// </summary>
        /// <param name="collection">
        ///   The extended service collection.
        /// </param>
        /// <param name="isSilentModeAllowed">
        ///   (optional; default=<c>true</c>)<br/>
        ///   When set the service collection is also configured for token caching and the OAuth2 Refresh Token grant. 
        /// </param>
        public static IServiceCollection AddTetraPakDeviceCodeGrant(
            this IServiceCollection collection, 
            bool isSilentModeAllowed = true)
        {
            lock (s_syncRoot)
            {
                if (s_isClientCredentialsAdded)
                    return collection;

                s_isClientCredentialsAdded = true;
            }
            
            collection.AddTetraPakConfiguration();
            collection.AddTetraPakHttpClientProvider();
            collection.AddSingleton<IDeviceCodeGrantService,TetraPakDeviceCodeGrantService>();
            if (isSilentModeAllowed)
            {
                collection.AddTetraPakRefreshTokenGrant();
            }
            return collection;
        }
    }
}