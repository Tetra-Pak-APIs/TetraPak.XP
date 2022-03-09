using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2.DeviceCode
{
    /// <summary>
    ///   Provides convenient methods for configuring Device Code Auth flow.
    /// </summary>
    public static class DeviceCodeServiceHelper
    {
        static bool s_isClientCredentialsAdded;
        static readonly object s_syncRoot = new();

        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds support for OAuth Device Code grant to the application and returns the <paramref name="collection"/>. 
        /// </summary>
        public static IServiceCollection UseTetraPakDeviceCodeAuthentication(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isClientCredentialsAdded)
                    return collection;

                s_isClientCredentialsAdded = true;
            }
            
            collection.UseTetraPakConfiguration();
            collection.UseTetraPakHttpClientProvider();
            collection.AddSingleton<IDeviceCodeGrantService,TetraPakDeviceCodeGrantService>();
            return collection;
        }
    }
}