using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.OAuth2.AuthCode;
using TetraPak.XP.Web;
using TetraPak.XP.Web.Abstractions;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2.OIDC
{
    public static class OidcServiceHelper
    {
        static readonly object s_syncRoot = new();
        // static bool s_isAuthApplicationAdded; obsolete
        static AuthApplication? s_authApplication;

        /// <summary>
        ///   Changes the current <see cref="AuthApplication"/>. 
        /// </summary>
        /// <param name="authApplication">
        ///   The new <see cref="AuthApplication"/> to be used.
        /// </param>
        public static void SetAuthApplication(AuthApplication authApplication) => s_authApplication = authApplication;

        // public static IServiceCollection AddAuthApplication(this IServiceCollection services, AuthApplication authApplication) obsolete 
        // {
        //     lock (s_syncRoot)
        //     {
        //         if (s_isAuthApplicationAdded)
        //             return services;
        //
        //         SetAuthApplication(authApplication);
        //         services.AddTransient(_ => s_authApplication!);
        //         s_isAuthApplicationAdded = true;
        //         return services;
        //     }
        // }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds Tetra Pak OIDC authentication to the application and returns the <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">
        ///   The dependency injection services collection. 
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logger provider.
        /// </param>
        /// <returns>
        ///   The <paramref name="collection"/> value.   
        /// </returns>
        /// <seealso cref="SetAuthApplication"/>
        public static IServiceCollection AddTetraPakOidcGrant(
            this IServiceCollection collection,
            ILog? log = null)
        {
            // todo The OIDC browser should be provider thru DI, not as a type
            // todo The OIDC service should take its config from IConfiguration (provided by DI); not from AuthApplication
            XpServices.RegisterLiteral<DiscoveryDocumentCache>();
            collection.AddTetraPakConfiguration();
            collection.AddTetraPakDiscoveryDocument();
            collection.AddTetraPakHttpClientProvider();
            collection.RegisterXpServices(log);
            collection.AddSingleton(p => AuthConfig.Default(
                p.GetRequiredService<AuthApplication>(),
                p.GetRequiredService<ILoopbackBrowser>(),
                p.GetService<ILog>()));
            collection.AddSingleton<IAuthorizationCodeGrantService, TetraPakAuthorizationCodeGrantService>();
            
            return collection;
        }
    }
}