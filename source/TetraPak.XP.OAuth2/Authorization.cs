using System;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging;
using TetraPak.XP.Web;


namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   Provides a starting point for authorization.
    /// </summary>
    /// <remarks>
    ///   This class can be used to start authorization of your client.
    /// </remarks>
    public static class Authorization
    {
        static Func<IAuthorizingAppDelegate>? s_appDelegate;

        /// <summary>
        ///   Intended for internal use only.
        /// </summary>
        public static bool IsAppDelegateRegistered => s_appDelegate != null;

        /// <summary>
        ///   Intended for internal use only.
        /// </summary>
        public static void RegisterAppDelegate(Func<IAuthorizingAppDelegate> appDelegate)
        {
            if (s_appDelegate != null)
                throw new InvalidOperationException("Authorizing App Delegate was already registered");

            s_appDelegate = appDelegate;
            TetraPakAuthenticator.Authorized += onAuthorized;
        }

        static async void onAuthorized(object sender, AuthResultEventArgs args)
        {
            if (!args.Result) 
                return;
            
            var cache = XpServices.Get<DiscoveryDocumentCache>(); 
            await DiscoveryDocument.TryDownloadAndSetCurrentAsync(args.Result.Value!, cache);
        }

        /// <summary>
        ///   Intended for internal use only.
        /// </summary>
        public static IAuthorizingAppDelegate? GetAuthorizingAppDelegate() => s_appDelegate?.Invoke() ?? null;

        /// <summary>
        ///   Resolves and returns a suitable authenticator.  
        /// </summary>
        /// <param name="config">
        ///   The app configuration (<see cref="AuthConfig"/>).
        /// </param>
        /// <returns>
        ///   An authenticator (implements <see cref="IAuthenticator"/>).
        /// </returns>
        public static IAuthenticator GetAuthenticator(AuthConfig config) 
            => new TetraPakAuthenticator(config);

        /// <summary>
        ///   Resolves and returns a suitable authenticator. 
        /// </summary>
        /// <param name="application">
        ///   A <see cref="AuthApplication"/> descriptor value.
        ///   Can be passed as a string.
        /// </param>
        /// <param name="browser">
        ///   An interactive browser service.
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logger, for diagnostics purposes. 
        /// </param>
        /// <param name="cache">
        ///   (optional)<br/>
        ///   A caching provider.
        /// </param>
        /// <returns>
        ///   An authenticator (implements <see cref="IAuthenticator"/>).
        /// </returns>
        public static IAuthenticator GetAuthenticator(
            AuthApplication application, 
            ILoopbackBrowser browser,
            ILog? log = null,
            ITimeLimitedRepositories? cache = null)
        {
            return GetAuthenticator(AuthConfig.Default(application, browser, log).WithCache(cache));
        }

        /// <summary>
        ///   Resolves and returns a suitable authenticator.
        /// </summary>
        /// <param name="clientId">
        ///   Specifies the client id (a.k.a. app id). 
        /// </param>
        /// <param name="redirectUri">
        ///   Specifies the redirect <see cref="Uri"/>.
        /// </param>
        /// <param name="browser">
        ///   An interactive browser service.
        /// </param>
        /// <param name="environment">
        ///   (optional; default = <see cref="RuntimeEnvironment.Production"/>)<br/>
        ///   Specifies the targeted runtime environment. 
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logger, for diagnostics purposes. 
        /// </param>
        /// <returns>
        ///   An authenticator (implements <see cref="IAuthenticator"/>).
        /// </returns>
        public static IAuthenticator GetAuthenticator(
            string clientId,
            Uri redirectUri, 
            ILoopbackBrowser browser,
            RuntimeEnvironment environment = RuntimeEnvironment.Production, 
            ILog? log = null)
            => GetAuthenticator(AuthConfig.Default(environment, clientId, redirectUri, browser, log: log));
    }
}
