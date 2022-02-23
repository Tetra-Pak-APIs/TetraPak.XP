using System;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth
{
    public class ServiceAuthConfigSectionWrapper : ConfigurationSectionWrapper, IServiceAuthConfig
    {
        readonly IRuntimeEnvironmentResolver _environmentResolver;
        string? _authDomain;
        string? _tokenIssuerUrl;
        string? _deviceCodeIssuerUrl;
        bool? _isCaching;

        public ITokenCache? TokenCache { get; }
        
        public bool IsCaching
        {
            get => _isCaching ?? Section?.Get<bool>() ?? false;
            set => setIsCaching(value);
        }
        
        async void setIsCaching(bool? value)
        {
            _isCaching = value;
            if (IsCaching && ClientId is {})
                await TokenCache?.DeleteAsync(ClientId)!;
        }
        
        /// <summary>
        ///   Gets the current runtime environment (DEV, TEST, PROD ...).
        ///   The value is a <see cref="RuntimeEnvironment"/> enum value. 
        /// </summary>
        [StateDump]
        public RuntimeEnvironment Environment
        {
            get
            {
                var resolved = _environmentResolver.ResolveRuntimeEnvironment();
                return resolved == RuntimeEnvironment.Unknown
                    ? Get<RuntimeEnvironment?>() ?? RuntimeEnvironment.Production
                    : resolved;
            }
        } 

        /// <summary>
        ///   Gets the resource locator for the token issuer endpoint.  
        /// </summary>
        [StateDump]
        public string TokenIssuerUrl
        {
            get => _tokenIssuerUrl ?? defaultUrl("/oauth2/token");
            set => _tokenIssuerUrl = value;
        }
        
        /// <summary>
        ///   Gets the resource locator for the token issuer endpoint.  
        /// </summary>
        [StateDump]
        public string DeviceCodeIssuerUrl
        {
            get => _deviceCodeIssuerUrl ?? defaultUrl("/oauth2/device_authorization");
            set => _deviceCodeIssuerUrl = value;
        }
        
        /// <summary>
        ///   Gets the domain element of the authority URI.
        /// </summary>
        [StateDump]
        public string AuthDomain
        {
            get => OnGetAuthDomain();
            set => _authDomain = value;
        }
        
        public GrantType GrantType => Section?.Get<GrantType>() ?? GrantType.None;
        
        public string? ClientId => Section?.Get<string?>();
        
        public string? ClientSecret => Section?.Get<string?>();

        public MultiStringValue? Scope => Section?.Get<MultiStringValue?>();
        
        public Task<Outcome<string>> GetClientIdAsync(AuthContext authContext)
        {
            throw new NotImplementedException();
        }

        public Task<Outcome<string>> GetClientSecretAsync(AuthContext authContext)
        {
            throw new NotImplementedException();
        }

        public Task<Outcome<Uri>> GetRedirectUriAsync(AuthContext authContext)
        {
            throw new NotImplementedException();
        }

        public Task<Outcome<GrantScope>> GetScopeAsync(AuthContext authContext, MultiStringValue? useDefault = null,
            CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Outcome<bool>> IsStateUsedAsync(AuthContext authContext, bool useDefault, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Outcome<bool>> IsPkceUsedAsync(AuthContext authContext, bool useDefault, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public string? GetConfiguredValue(string key)
        {
            throw new NotImplementedException();
        }

        public Task<Outcome<Uri>> GetTokenIssuerUrlAsync(AuthContext authContext)
        {
            // todo consider supporting getting from well-known discovery document
            return Task.FromResult(Outcome<Uri>.Success(new Uri(TokenIssuerUrl)));
        }

        public Task<Outcome<Uri>> GetAuthorityUrlAsync(AuthContext authContext)
        {
            // todo consider supporting getting from well-known discovery document
            return Task.FromResult(Outcome<Uri>.Success(new Uri(TokenIssuerUrl)));
        }

        public Task<Outcome<Uri>> GetDeviceCodeIssuerUrlAsync(AuthContext authContext)
        {
            // todo consider supporting getting from well-known discovery document
            return Task.FromResult(Outcome<Uri>.Success(new Uri(DeviceCodeIssuerUrl)));
        }

        public bool IsAuthIdentifier(string identifier)
        {
            throw new NotImplementedException();
        }
        
        protected virtual string OnGetAuthDomain()
        {
            if (_authDomain is { })
                return _authDomain;

            return Environment switch
            {
                RuntimeEnvironment.Production => "https://api.tetrapak.com",
                RuntimeEnvironment.Migration => "https://api-mig.tetrapak.com",
                RuntimeEnvironment.Development => "https://api-dev.tetrapak.com",
                RuntimeEnvironment.Sandbox => "https://api-sb.tetrapak.com",
                RuntimeEnvironment.Unknown => throw new NotSupportedException($"Runtime environment is unresolved"),
                _ => throw new NotSupportedException($"Unsupported runtime environment: {Environment}")
            };
        }
        
        string defaultUrl(string path) => $"{AuthDomain}{path}";

        public ServiceAuthConfigSectionWrapper(
            IConfigurationSectionExtended section, 
            IRuntimeEnvironmentResolver environmentResolver,
            ITokenCache? tokenCache = null,
            ILog? log = null) 
        : base(section, log)
        {
            _environmentResolver = environmentResolver;
            TokenCache = tokenCache;
        }

        protected ServiceAuthConfigSectionWrapper(   
            IConfiguration? configuration,
            string key,
            IRuntimeEnvironmentResolver environmentResolver,
            ITokenCache? tokenCache = null,
            ILog? log = null) 
        : base(configuration, key, log)
        {
            _environmentResolver = environmentResolver;
            TokenCache = tokenCache;
        }
    }
}