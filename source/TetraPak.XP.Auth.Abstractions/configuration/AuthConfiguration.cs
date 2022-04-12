using Microsoft.Extensions.Configuration;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.Configuration;
using TetraPak.XP.Diagnostics;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   A typed <see cref="IConfigurationSection"/> that implements the <see cref="IAuthConfiguration"/>. 
    /// </summary>
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
                var value = this.Get<string?>(getDerived: true);
                return !string.IsNullOrWhiteSpace(value) 
                    ? value! 
                    : TetraPakAuthDefaults.Domain(RuntimeEnvironment);

                // return RuntimeEnvironment switch obsolete
                // {
                //     RuntimeEnvironment.Production => TetraPakAuthDefaults.ProductionDomain,
                //     RuntimeEnvironment.Migration => TetraPakAuthDefaults.MigrationDomain,
                //     RuntimeEnvironment.Development => TetraPakAuthDefaults.DevelopmentDomain,
                //     RuntimeEnvironment.Sandbox => TetraPakAuthDefaults.SandboxDomain,
                //     RuntimeEnvironment.Unknown => throw error(),
                //     _ => throw error()
                // };
                //
                // ConfigurationException error()
                //     => new($"Could not resolve authority domain from runtime environment '{RuntimeEnvironment}'");
            }
        }

        /// <inheritdoc />
        [StateDump]
        public string AuthorityUri
        {
            get
            {
                var value = this.Get<string?>(getDerived: true);
                return string.IsNullOrWhiteSpace(value) 
                    ? $"{TetraPakAuthDefaults.AuthorityUri(RuntimeEnvironment)}" 
                    : value!;
            }
        }

        /// <inheritdoc />
        [StateDump]
        public string TokenIssuerUri 
        {
            get
            {
                var value = this.Get<string?>(getDerived: true);
                return string.IsNullOrWhiteSpace(value)
                    ? $"{TetraPakAuthDefaults.TokenIssuerUri(RuntimeEnvironment)}"
                    : value!;
            }
        }

        /// <inheritdoc />
        [StateDump]
        public string DeviceCodeIssuerUri 
        {
            get
            {
                var value = this.Get<string?>(getDerived: true);
                return string.IsNullOrWhiteSpace(value) ? $"{TetraPakAuthDefaults.DeviceCodeIssuerUri(RuntimeEnvironment)}" : value!;
            }
        }

        /// <inheritdoc />
        [StateDump]
        public string? RedirectUri => this.Get<string>(getDerived:true);

        /// <inheritdoc />
        public string DiscoveryDocumentUri
        {
            get
            {
                var value = this.Get<string?>(getDerived: true);
                return string.IsNullOrWhiteSpace(value) ? $"{AuthDomain}{DiscoveryDocument.DefaultPath}" : value!;
            }
        }

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