using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.ClientCredentials;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth.DeviceCode
{
    public static class DeviceCodeServiceHelper
    {
        static bool s_isClientCredentialsAdded;
        static readonly object s_syncRoot = new();

        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds support for OAuth Device Code grant to the application and returns the <paramref name="collection"/>. 
        /// </summary>
        public static IServiceCollection AddTetraPakDeviceCodeAuthentication(this IServiceCollection collection)
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
            return collection;
        }
    }
}