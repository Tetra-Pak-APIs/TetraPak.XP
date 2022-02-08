using Microsoft.Extensions.DependencyInjection;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth
{
    public static class ClientCredentialsServiceHelper
    {
        static bool s_isClientCredentialsAdded;
        static readonly object s_syncRoot = new();

        
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