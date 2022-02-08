using System;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth
{
    public class TetraPakConfig : ConfigurationSectionWrapper, ITetraPakConfiguration
    {
        string? _authDomain;
        string? _tokenIssuerUrl;
        readonly IRuntimeEnvironmentResolver _environmentResolver;
        const string SectionKey = "TetraPak";
        
        public GrantType GrantType => Section?.Get<GrantType>() ?? GrantType.None;
        public string? ClientId => Section?.Get<string?>();
        public string? ClientSecret => Section?.Get<string?>();

        /// <summary>
        ///   Gets the current runtime environment (DEV, TEST, PROD ...).
        ///   The value is a <see cref="RuntimeEnvironment"/> enum value. 
        /// </summary>
        [StateDump]
        public RuntimeEnvironment Environment 
            =>
            Get<RuntimeEnvironment?>() ?? _environmentResolver.ResolveRuntimeEnvironment();
        
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

        public Task<Outcome<MultiStringValue>> GetScopeAsync(AuthContext authContext, MultiStringValue? useDefault = null,
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
        
        public TetraPakConfig(IConfigurationSectionExtended section, ILog? log) 
        : base(section, log)
        {
        }

        public TetraPakConfig(IConfiguration? configuration, IRuntimeEnvironmentResolver environmentResolver, ILog? log = null) 
        : base(configuration, SectionKey, log)
        {
            _environmentResolver = environmentResolver;
        }
    }
}