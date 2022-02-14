using Microsoft.Extensions.DependencyInjection;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth.ClientCredentials
{
    /// <summary>
    ///   Helps with adding support for the Client Credentials grant type
    ///   (see <see cref="AddTetraPakClientCredentialsAuthentication"/>). 
    /// </summary>
    public static class ClientCredentialsServiceHelper
    {
        static bool s_isClientCredentialsAdded;
        static readonly object s_syncRoot = new();

        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds support for OAuth Client Credentials grant to the application and returns the <paramref name="collection"/>. 
        /// </summary>
        public static IServiceCollection AddTetraPakClientCredentialsAuthentication(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isClientCredentialsAdded)
                    return collection;

                s_isClientCredentialsAdded = true;
            }
            
            collection.AddTetraPakConfiguration();
            collection.AddTetraPakHttpClientProvider();
            collection.AddSingleton<IClientCredentialsGrantService,TetraPakClientCredentialsGrantService>();
            return collection;
        }
    }
}