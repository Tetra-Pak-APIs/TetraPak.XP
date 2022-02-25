using System;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth
{
    public class ServiceAuthConfigSectionWrapper : ConfigurationSectionWrapper, IServiceAuthConfig
    {
        string? _authDomain;
        string? _tokenIssuerUrl;
        string? _deviceCodeIssuerUrl;
        bool? _isCaching;

        IServiceAuthConfig AuthConfigSection => (IServiceAuthConfig)Section!;

        protected RuntimeEnvironment Environment => AuthConfigSection.GetDerived<RuntimeEnvironment>(); 

        
        /// <inheritdoc />
        [StateDump]
        public string? AuthorityUri => AuthConfigSection.AuthorityUri;

        /// <summary>
        ///   Gets the resource locator for the token issuer endpoint.  
        /// </summary>
        [StateDump]
        public string TokenIssuerUri
        {
            get => _tokenIssuerUrl ?? defaultUrl("/oauth2/token");
            set => _tokenIssuerUrl = value;
        }
        
        /// <summary>
        ///   Gets the resource locator for the token issuer endpoint.  
        /// </summary>
        [StateDump]
        public string DeviceCodeIssuerUri
        {
            get => _deviceCodeIssuerUrl ?? defaultUrl("/oauth2/device_authorization");
            set => _deviceCodeIssuerUrl = value;
        }

        public string? RedirectUri => AuthConfigSection.RedirectUri;

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
        ///   Gets the domain element of the authority URI.
        /// </summary>
        [StateDump]
        public string AuthDomain
        {
            get => OnGetAuthDomain();
            set => _authDomain = value;
        }
        
        public virtual GrantType GrantType => AuthConfigSection.GrantType;
        
        public string? ClientId => AuthConfigSection.ClientId;
        
        public string? ClientSecret => AuthConfigSection.ClientSecret;

        public GrantScope? Scope => AuthConfigSection.Scope;

        public bool UseState => AuthConfigSection.UseState;

        public bool UsePKCE => AuthConfigSection.UsePKCE;

        public string? GetRawConfiguredValue(string key) => AuthConfigSection.GetRawConfiguredValue(key);

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
            IConfigurationSection section, 
            ITokenCache? tokenCache = null,
            ILog? log = null) 
        : base(section, log)
        {
            TokenCache = tokenCache;
        }

        // protected ServiceAuthConfigSectionWrapper(   
        //     IConfiguration? configuration,
        //     string key,
        //     ITokenCache? tokenCache = null,
        //     ILog? log = null) 
        // : base(configuration, key, log)
        // {
        //     TokenCache = tokenCache;
        // }
    }
}