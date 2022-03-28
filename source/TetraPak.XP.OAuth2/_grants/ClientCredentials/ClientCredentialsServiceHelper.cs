using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2.ClientCredentials
{
    /// <summary>
    ///   Helps with adding support for the Client Credentials grant type
    ///   (see <see cref="AddTetraPakClientCredentialsGrant"/>). 
    /// </summary>
    public static class ClientCredentialsServiceHelper
    {
        static bool s_isClientCredentialsAdded;
        static readonly object s_syncRoot = new();

        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds support for OAuth Client Credentials grant to the application and returns the <paramref name="collection"/>. 
        /// </summary>
        public static IServiceCollection AddTetraPakClientCredentialsGrant(this IServiceCollection collection)
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