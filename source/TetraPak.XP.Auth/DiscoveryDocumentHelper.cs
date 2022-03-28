using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions.OIDC;

namespace TetraPak.XP.Auth
{
    public static class DiscoveryDocumentHelper
    {
        static readonly object s_syncRoot = new();
        static bool s_isDiscoveryDocumentServiceAdded;
        
        public static IServiceCollection AddTetraPakDiscoveryDocument(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isDiscoveryDocumentServiceAdded)
                    return collection;

                s_isDiscoveryDocumentServiceAdded = true;
            }

            collection.AddSingleton<IDiscoveryDocumentProvider, TetraPakDiscoveryDocumentProvider>();
            return collection;
        }
    }
}