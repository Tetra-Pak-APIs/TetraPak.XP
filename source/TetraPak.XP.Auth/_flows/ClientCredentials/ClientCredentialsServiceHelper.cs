using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth
{
    public static class ClientCredentialsServiceHelper
    {
        static bool s_isClientCredentialsAdded;
        static readonly object s_syncRoot = new();

        
        public static IServiceCollection AddTetraClientCredentialsAuthentication(
            this IServiceCollection collection,
            AuthApplication? authApplication = null,
            ILog? log = null)
        {
            lock (s_syncRoot)
            {
                if (s_isClientCredentialsAdded)
                    return collection;

                s_isClientCredentialsAdded = true;
            }
            
            collection.AddTetraPakConfiguration();
            collection.AddSingleton<IClientCredentialsGrantService,TetraPakClientCredentialsGrantService>();
            return collection;
        }
    }
}