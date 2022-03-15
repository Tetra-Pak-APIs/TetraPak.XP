using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth.Abstractions
{
    public class AuthConfiguration : ConfigurationSectionDecorator, IAuthConfiguration
    {
        internal IRuntimeEnvironmentResolver RuntimeEnvironmentResolver { get; }

        public virtual RuntimeEnvironment RuntimeEnvironment => this.GetFromRoot<RuntimeEnvironment>();

        /// <inheritdoc />
        [StateDump]
        public string AuthDomain
        {
            get
            {
                var value = this.Get<string>(getDerived: true);
                if (!string.IsNullOrWhiteSpace(value))
                    return value!;

                return RuntimeEnvironment switch
                {
                    RuntimeEnvironment.Production => TetraPakAuthDefaults.ProductionDomain,
                    RuntimeEnvironment.Migration => TetraPakAuthDefaults.MigrationDomain,
                    RuntimeEnvironment.Development => TetraPakAuthDefaults.DevelopmentDomain,
                    RuntimeEnvironment.Sandbox => TetraPakAuthDefaults.SandboxDomain,
                    RuntimeEnvironment.Unknown => throw error(),
                    _ => throw error()
                };

                ConfigurationException error()
                    => new($"Could not resolve authority domain from runtime environment '{RuntimeEnvironment}'");
            }
        }

        /// <inheritdoc />
        [StateDump]
        public string AuthorityUri
        {
            get
            {
                var value = this.Get<string>(getDerived: true);
                return string.IsNullOrWhiteSpace(value) ? $"{AuthDomain}{TetraPakAuthDefaults.DefaultAuthorityPath}" : value!;
            }
        }

        /// <inheritdoc />
        [StateDump]
        public string TokenIssuerUri 
        {
            get
            {
                var value = this.Get<string>(getDerived: true);
                return string.IsNullOrWhiteSpace(value) ? $"{AuthDomain}{TetraPakAuthDefaults.DefaultTokenIssuerPath}" : value!;
            }
        }

        /// <inheritdoc />
        [StateDump]
        public string DeviceCodeIssuerUri 
        {
            get
            {
                var value = this.Get<string>(getDerived: true);
                return string.IsNullOrWhiteSpace(value) ? $"{AuthDomain}{TetraPakAuthDefaults.DefaultDeviceCodeIssuerPath}" : value!;
            }
        }

        /// <inheritdoc />
        [StateDump]
        public string? RedirectUri => this.Get<string>(getDerived:true);

        /// <inheritdoc />
        [StateDump]
        public GrantType GrantType => this.Get<GrantType?>(getDerived:true) ?? GrantType.None;

        /// <inheritdoc />
        [StateDump]
        public string? ClientId => this.Get<string?>(getDerived:true);

        /// <inheritdoc />
        [StateDump]
        public string? ClientSecret => this.Get<string>(getDerived:true);

        /// <inheritdoc />
        [StateDump]
        public GrantScope? OidcScope => this.Get(getDerived:true, useDefault: TetraPakAuthDefaults.OidcScope);
        
        /// <inheritdoc />
        [StateDump]
        public bool OidcState => this.Get(getDerived:true, useDefault: TetraPakAuthDefaults.OidcState);
        
        /// <inheritdoc />
        [StateDump]
        public bool OidcPkce => this.Get(getDerived:true, useDefault: TetraPakAuthDefaults.OidcPkce);

        /// <inheritdoc />
        [StateDump]
        public bool IsCaching => this.Get(getDerived:true, useDefault: TetraPakAuthDefaults.IsCaching);
        
        public AuthConfiguration(ConfigurationSectionDecoratorArgs args)
        : base(args)
        {
            RuntimeEnvironmentResolver = args.RuntimeEnvironmentResolver;
        }
    }
}