using System;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth
{
    public class TetraPakConfig : ConfigurationSectionWrapper, ITetraPakConfiguration
    {
        string? _authDomain;
        string? _tokenIssuerUrl;
        readonly IRuntimeEnvironmentResolver _environmentResolver;
        bool? _isCaching;
        readonly ITimeLimitedRepositories? _cache;
        readonly ITokenCache? _tokenCache;
        const string SectionKey = "TetraPak";
        
        public GrantType GrantType => Section?.Get<GrantType>() ?? GrantType.None;
        public string? ClientId => Section?.Get<string?>();
        public string? ClientSecret => Section?.Get<string?>();
        
        public bool IsCaching
        {
            get => _isCaching ?? Section?.Get<bool>() ?? false;
            set => setIsCaching(value);
        }
        
        async void setIsCaching(bool? value)
        {
            _isCaching = value;
            if (IsCaching && ClientId is {})
                await _tokenCache?.DeleteAsync(ClientId)!;
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
        public string TokenIssuerUrl => _tokenIssuerUrl ?? defaultUrl("/oauth2/token");

        public Task<Uri> GetTokenIssuerUrlAsync()
        {
            // todo consider supporting getting from well-known discovery document
            return Task.FromResult(new Uri(_tokenIssuerUrl ?? defaultUrl("/oauth2/token")));
        }
        
        public Task<Uri> GetDeviceCodeIssuerUrlAsync()
        {
            // todo consider supporting getting from well-known discovery document
            return Task.FromResult(new Uri(_tokenIssuerUrl ?? defaultUrl("/oauth2/device_authorization")));
        }
        
        string defaultUrl(string path) => $"{AuthDomain}{path}";
        
        /// <summary>
        ///   Gets the domain element of the authority URI.
        /// </summary>
        [StateDump]
        public string AuthDomain
        {
            get
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
            set => _authDomain = value;
        }

        public MultiStringValue? Scope => Section?.Get<MultiStringValue?>();

        public string? RequestMessageIdHeader => Section?.Get<string?>();

        
        public Task<Outcome<string>> GetClientIdAsync(AuthContext authContext, CancellationToken? cancellationToken = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<Outcome<string>> GetClientSecretAsync(AuthContext authContext, CancellationToken? cancellationToken = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<Outcome<GrantScope>> GetScopeAsync(AuthContext authContext, MultiStringValue? useDefault = null,
            CancellationToken? cancellationToken = null)
        {
            throw new System.NotImplementedException();
        }

        public string? GetConfiguredValue(string key)
        {
            throw new System.NotImplementedException();
        }

        public ConfigPath? ConfigPath => Section?.Get<ConfigPath?>();
        
        public bool IsAuthIdentifier(string identifier)
        {
            throw new System.NotImplementedException();
        }
        
        public TetraPakConfig(
            IConfigurationSectionExtended section, 
            IRuntimeEnvironmentResolver environmentResolver,
            ITimeLimitedRepositories? cache = null,
            ITokenCache? tokenCache = null,
            ILog? log = null) 
        : base(section, log)
        {
            _cache = cache;
            _tokenCache = tokenCache;
            _environmentResolver = environmentResolver;
        }

        public TetraPakConfig(
            IConfiguration? configuration,
            IRuntimeEnvironmentResolver environmentResolver,
            ITimeLimitedRepositories? cache = null,
            ITokenCache? tokenCache = null,
            ILog? log = null) 
        : base(configuration, SectionKey, log)
        {
            _cache = cache;
            _tokenCache = tokenCache;
            _environmentResolver = environmentResolver;
        }
    }
}