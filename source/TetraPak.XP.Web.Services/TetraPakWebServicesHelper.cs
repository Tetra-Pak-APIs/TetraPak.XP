using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP.Web.Services
{
    
    // todo move to a more "web" oriented lib
    public static class TetraPakWebServicesHelper
    {
        static WebServicesConfiguration? s_singleton;

        public static IServiceCollection AddTetraPakWebServices(this IServiceCollection collection) 
        {
            WebServicesConfiguration.InsertWrapperDelegates();
            // TetraPakTetraPakWebServices.InsertWebServiceValueDelegate();
            collection.AddSingleton<IWebServicesConfiguration>(_ => s_singleton!);
            return collection;
        }

        internal static void SetAsSingletonService(this WebServicesConfiguration webServicesConfiguration)
        {
            s_singleton = webServicesConfiguration;
        }
    }
}