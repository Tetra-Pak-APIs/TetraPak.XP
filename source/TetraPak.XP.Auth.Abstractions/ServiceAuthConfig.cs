using System;
using System.Threading.Tasks;
using TetraPak.XP.Configuration;

namespace TetraPak.XP.Auth.Abstractions
{
    class ServiceAuthConfig : ConfigurationSection, IServiceAuthConfig
    {
        public string AuthDomain => GetDerived<string>() ?? throw MissingConfigurationException(this, nameof(AuthDomain));
        
        public string? AuthorityUri => GetDerived<string>();
        
        public string? TokenIssuerUri => GetDerived<string>();
        
        public string? DeviceCodeIssuerUri => GetDerived<string>();
        
        public string? RedirectUri => GetDerived<string>();
        
        public string? ClientId => GetDerived<string>();
        
        public string? ClientSecret => GetDerived<string>();
        
        public GrantScope Scope => GetDerived<GrantScope>() ?? GrantScope.Empty;
        
        public GrantType GrantType => GetDerived<GrantType>();

        public bool UseState => GetDerived<bool>();

        public bool UsePKCE => GetDerived<bool>();
        
        public Task<Outcome<Credentials>> GetClientCredentialsAsync(bool isSecretRequired)
        {
            var clientId = GetDerivedValue<string>(nameof(ClientId));
            var clientSecret = GetDerivedValue<string?>(nameof(ClientId));
            if (string.IsNullOrWhiteSpace(clientId))
                return Task.FromResult(Outcome<Credentials>.Fail(new ConfigurationException($"Client credentials was not configured ({Path})")));
            
            if (isSecretRequired && string.IsNullOrWhiteSpace(clientSecret))
                return Task.FromResult(Outcome<Credentials>.Fail(new ConfigurationException($"Client secret was not configured ({Path})")));
            
            return Task.FromResult(Outcome<Credentials>.Success(new Credentials(clientId!, clientSecret)));
        }

        public string? GetRawConfiguredValue(string key)
        {
            throw new NotImplementedException();
        }
        
        internal static Outcome<T> MissingConfigurationOutcome<T>(IConfigurationSection cfg, string key) 
            => 
            Outcome<T>.Fail(MissingConfigurationException(cfg, key));
        
        internal static Exception MissingConfigurationException(IConfigurationSection cfg, string key)
            =>
            new ConfigurationException($"Missing configuration: {new ConfigPath(cfg.Path).Push(key)}");

        internal static Outcome<T> InvalidConfigurationOutcome<T>(IConfigurationSection cfg, string key, object value) 
            => 
            Outcome<T>.Fail(InvalidConfigurationException(cfg, key, value));

        internal static Exception InvalidConfigurationException(IConfigurationSection cfg, string key, object value)
            =>
            new ConfigurationException($"Invalid configuration: {new ConfigPath(cfg.Path).Push(key)}: {value}");

        public bool IsAuthIdentifier(string identifier)
        {
            return identifier switch
            {
                nameof(AuthorityUri) => true,
                nameof(TokenIssuerUri) => true,
                nameof(DeviceCodeIssuerUri) => true,
                nameof(RedirectUri) => true,
                nameof(GrantType) => true,
                nameof(ClientId) => true,
                nameof(ClientSecret) => true,
                nameof(Scope) => true,
                _ => false
            };
        }

        public ServiceAuthConfig(IConfiguration configuration, string key)
        {
        }
    }
}