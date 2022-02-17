using System;
using System.Threading.Tasks;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging;
using TetraPak.XP.Web;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   Used to configure a <see cref="TetraPakAuthenticator"/>.
    /// </summary>
    /// <remarks>
    ///   Used to configure a `IAuthenticator` used for acquiring access tokens.
    ///   The <b>TAX</b> system can create this configuration behind the scene if you describe your application
    ///   using the more simplified <see cref="AuthApplication"/> when invoking
    ///   <see cref="Authorization.GetAuthenticator(AuthConfig)"/>
    ///   but an `AuthConfig` allows for more custom configuration.
    /// </remarks>
    public class AuthConfig
    {
        const string DevelopmentAuthorityUrl = "https://api-dev.tetrapak.com/oauth2/authorize";
        const string MigrationAuthorityUrl = "https://api-mig.tetrapak.com/oauth2/authorize";
        const string ProductionAuthorityUrl = "https://api.tetrapak.com/oauth2/authorize";
        
        const string DevelopmentTokenIssuerUrl = "https://api-dev.tetrapak.com/oauth2/token";
        const string MigrationTokenIssuerUrl = "https://api-mig.tetrapak.com/oauth2/token";
        const string ProductionTokenIssuerUrl = "https://api.tetrapak.com/oauth2/token";
        
        internal const string DevelopmentUserInfoUrl = "https://api-dev.tetrapak.com/userinfo";
        internal const string MigrationUserInfoUrl = "https://api-mig.tetrapak.com/userinfo";
        internal const string ProductionUserInfoUrl = "https://api.tetrapak.com/userinfo";

        ITimeLimitedRepositories? _cache;
        ITokenCache? _tokenCache;
        Uri? _authority;
        Uri? _tokenIssuer;
#if DEBUG
        Uri? _localAuthority;
        Uri? _localTokenIssuer;
#endif
        bool _isCaching;

        public ILog? Log { get; private set; }

        public ILoopbackBrowser Browser { get; }

        /// <summary>
        ///   Gets a value indicating whether user identity will be requested during the auth flow. 
        /// </summary>
        public bool IsRequestingUserId { get; set; }

        /// <summary>
        ///   Gets or sets the <see cref="Uri"/> to the authority endpoint.
        /// </summary>
        public Uri? Authority
        {
#if DEBUG
            get => IsTargetingLocalAuthority ? _localAuthority : _authority;
            set
            {
                if (IsTargetingLocalAuthority)
                    _localAuthority = value;
                else
                    _authority = value;
                _authority = value;
            }
#else
            get => _authority;
            set => _authority = value;
#endif
        }

        /// <summary>
        ///   Gets or sets the <see cref="Uri"/> to the token issuing endpoint.
        /// </summary>
        public Uri? TokenIssuer
        {
#if DEBUG
            get => IsTargetingLocalAuthority ? _localTokenIssuer : _tokenIssuer;
            set
            {
                if (IsTargetingLocalAuthority)
                    _localTokenIssuer = value;
                else
                    _tokenIssuer = value;
            }
#else
            get => _tokenIssuer;
            set => _tokenIssuer = value;
#endif
        }

        /// <summary>
        ///   Gets or sets a redirect <see cref="Uri"/>, used for passing back an auth code.
        /// </summary>
        public Uri? RedirectUri { get; set; }

        /// <summary>
        ///   Gets or sets the client id (a.k.a. "app id").
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        ///   Gets or sets a scope value when applicable.
        /// </summary>
        public AuthScope Scope { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use state in the auth code flow.
        /// </summary>
        public bool IsStateUsed { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use the PKCE extension with the auth code flow.
        /// </summary>
        public bool IsPkceUsed { get; set; }

#if DEBUG
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IsTargetingLocalAuthority { get; set; }
#endif

        /// <summary>
        ///   Gets or sets a value specifying whether authorizations (tokens)
        ///   should be cached. Default value is <c>true</c>.
        /// </summary>
        public bool IsCaching
        {
            get => _isCaching;
            set => setIsCaching(value);
        }

        async void setIsCaching(bool value)
        {
            _isCaching = value;
            if (!_isCaching)
                await _tokenCache?.DeleteAsync(ClientId)!;
        }

        /// <summary>
        ///   Gets or sets an arbitrary cache. 
        /// </summary>
        public ITimeLimitedRepositories? Cache
        {
            get => _cache;
            set => _cache = value ?? _cache;
        }
        
        /// <summary>
        ///   Gets or sets a (custom) token cache. This value cannot be unassigned
        ///   and a default token cache will always be provided.
        /// </summary>
        public ITokenCache? TokenCache
        {
            get => _tokenCache;
            set => _tokenCache = value ?? _tokenCache;
        }

        /// <summary>
        ///   Creates a default OAuth configuration for a specified application.
        /// </summary>
        /// <param name="environment">
        ///   The targeted runtime environment.
        /// </param>
        /// <param name="clientId">
        ///   The application's client id (a.k.a. "app id").
        /// </param>
        /// <param name="redirectUri">
        ///     The application's redirect <see cref="Uri"/>.
        /// </param>
        /// <param name="browser">
        ///   An interactive browser service.
        /// </param>
        /// <param name="platform">
        ///   (optional; default = <see cref="RuntimePlatform.Any"/>)<br/>
        ///   An intended runtime platform.
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logging provider.
        /// </param>
        /// <returns>
        ///   A <see cref="AuthConfig"/> object with a default configuration for the specified application.
        /// </returns>
        public static AuthConfig Default(
            RuntimeEnvironment environment, 
            string clientId, 
            Uri redirectUri, 
            ILoopbackBrowser browser,
            RuntimePlatform platform = RuntimePlatform.Any,
            ILog? log = null)
            => Default(new AuthApplication(clientId, redirectUri, environment, platform), browser, log);

        /// <summary>
        ///   Creates a default OAuth configuration for a specified application.
        /// </summary>
        /// <param name="application">
        ///     Describes the application to be authorized.
        /// </param>
        /// <param name="browser">
        ///   An interactive browser service.
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logging provider.
        /// </param>
        /// <returns>
        ///   A <see cref="AuthConfig"/> object with a default configuration for the specified application.
        /// </returns>
        public static AuthConfig Default(AuthApplication application, ILoopbackBrowser browser, ILog? log = null)
        {
            return new AuthConfig(
                getAuthority(application.Environment),
                getTokenIssuer(application.Environment),
                application.RedirectUri,
                application.ClientId, 
                browser,
                log:log);
        }

        static Uri getAuthority(RuntimeEnvironment environment)
        {
            return environment switch
            {
                RuntimeEnvironment.Development => new Uri(DevelopmentAuthorityUrl),
                //RuntimeEnvironment.Test => throw new NotSupportedException($"Unsupported environment: {environment}"), obsolete
                RuntimeEnvironment.Migration => new Uri(MigrationAuthorityUrl),
                RuntimeEnvironment.Production => new Uri(ProductionAuthorityUrl),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        static Uri getTokenIssuer(RuntimeEnvironment environment)
        {
            return environment switch
            {
                RuntimeEnvironment.Development => new Uri(DevelopmentTokenIssuerUrl),
                //RuntimeEnvironment.Test => throw new NotSupportedException($"Unsupported environment: {environment}"), obsolete
                RuntimeEnvironment.Migration => new Uri(MigrationTokenIssuerUrl),
                RuntimeEnvironment.Production => new Uri(ProductionTokenIssuerUrl),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        ///   Flags the authorization request to also request user identity.
        ///   This is often required by APIs when making requests for data. 
        /// </summary>
        /// <remarks>
        ///   This is mainly a clear-code way to ask for the <c>"openid"</c> scope.
        /// </remarks>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public AuthConfig WithUserIdentity()
        {
            IsRequestingUserId = true;
            return this;
        }

        public AuthConfig WithLogging(ILog log)
        {
            Log = log;
            return this;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        ///   Initializes the configuration.
        /// </summary>
        public AuthConfig(
            Uri authority, 
            Uri tokenIssuer, 
            Uri redirectUri, 
            string clientId, 
            ILoopbackBrowser browser,
            string scope = "", 
            bool isStateUsed = true, 
            bool isPkceUsed = true, 
            bool isCaching = true,
            ILog? log = null,
            ITimeLimitedRepositories? cache = null,
            ITokenCache? tokenCache = null)
        {
            Log = log;
            Authority = _authority = authority;
            TokenIssuer = _tokenIssuer = tokenIssuer;
            RedirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));
            ClientId = clientId.IsAssigned() ? clientId : throw new ArgumentNullException(nameof(clientId));
            Browser = browser;
            Scope = scope;
            IsStateUsed = isStateUsed;
            IsPkceUsed = isPkceUsed;
            IsCaching = isCaching;
            _cache = cache;
            _tokenCache = tokenCache?.WithDefaultKey<ITokenCache>(clientId);
        }

    }

    public static class AuthConfigHelper
    {
        public static async Task<Outcome<Grant>> GetCachedTokenAsync(this AuthConfig config, string cacheKey)
        {
            return config.IsCaching && config.TokenCache is {} 
                ? await config.TokenCache.ReadAsync<Grant>() 
                : Outcome<Grant>.Fail(new Exception());
        }
        
    }
}
