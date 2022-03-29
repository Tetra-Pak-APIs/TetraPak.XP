using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.Identity;

public static class UserInformationServiceHelper
{
    static readonly object s_syncRoot = new();
    static bool s_isUserInformationAdded;
    
    public static IServiceCollection AddTetraPakUserInformation(this IServiceCollection collection)
    {
        lock (s_syncRoot)
        {
            if (s_isUserInformationAdded)
                return collection;

            s_isUserInformationAdded = true;
        }
        collection.AddTetraPakConfiguration();
        collection.AddTetraPakHttpClientProvider();
        collection.AddTetraPakDiscoveryDocument();
        collection.AddSingleton<IUserInformationService,TetraPakUserInformationService>();
        return collection;
    }
}