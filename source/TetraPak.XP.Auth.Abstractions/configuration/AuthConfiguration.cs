using System;
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

        /// <inheritdoc />
        public virtual RuntimeEnvironment RuntimeEnvironment => this.GetFromRoot<RuntimeEnvironment>();

        /// <inheritdoc />
        public virtual BrowserExperience BrowserExperience => this.Get(useDefault: BrowserExperience.InternalSystem, getDerived: true);

        /// <inheritdoc />
        [StateDump]
        public string AuthDomain
        {
            get
            {
                var value = this.Get<string?>(getDerived: true);
                return !string.IsNullOrWhiteSpace(value)
#if NET5_0_OR_GREATER                
                    ? value
#else
                    ? value!
#endif
                    : TetraPakAuthDefaults.Domain(RuntimeEnvironment);
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
#if NET5_0_OR_GREATER
                    : value;
#else
                    : value!;
#endif
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
#if NET5_0_OR_GREATER
                    : value;
#else
                    : value!;
#endif
            }
        }

        /// <inheritdoc />
        [StateDump]
        public string DeviceCodeIssuerUri 
        {
            get
            {
                var value = this.Get<string?>(getDerived: true);
                return string.IsNullOrWhiteSpace(value)
                    ? $"{TetraPakAuthDefaults.DeviceCodeIssuerUri(RuntimeEnvironment)}" 
#if NET5_0_OR_GREATER
                    : value;
#else
                    : value!;
#endif
            }
        }

        /// <inheritdoc />
        [StateDump]
        public string? RedirectUri => this.Get<string>(getDerived: true);

        /// <inheritdoc />
        public string DiscoveryDocumentUri
        {
            get
            {
                var value = this.Get<string?>(getDerived: true);
                return string.IsNullOrWhiteSpace(value) 
                    ? $"{AuthDomain}{DiscoveryDocument.DefaultPath}" 
#if NET5_0_OR_GREATER
                    : value;
#else
                    : value!;
#endif
            }
        }

        /// <inheritdoc />
        [StateDump]
        public GrantType GrantType => this.Get<GrantType?>(getDerived: true) ?? GrantType.None;

        /// <inheritdoc />
        [StateDump]
        public string? ClientId => this.Get<string?>(getDerived: true);

        /// <inheritdoc />
        [StateDump]
        public string? ClientSecret => this.Get<string>(getDerived: true);

        /// <inheritdoc />
        [StateDump]
        public GrantScope? OidcScope => this.Get(useDefault: TetraPakAuthDefaults.OidcScope, getDerived: true);
        
        /// <inheritdoc />
        [StateDump]
        public bool OidcState => this.Get(useDefault: TetraPakAuthDefaults.OidcState, getDerived: true);
        
        /// <inheritdoc />
        [StateDump]
        public bool OidcPkce => this.Get(useDefault: TetraPakAuthDefaults.OidcPkce, getDerived: true);

        /// <inheritdoc />
        [StateDump]
        public bool IsCaching => this.Get(useDefault: TetraPakAuthDefaults.IsCaching, getDerived: true);

        public TimeSpan? Timeout => this.Get<TimeSpan?>(getDerived: true);
        
        public AuthConfiguration(ConfigurationSectionDecoratorArgs args)
        : base(args)
        {
            RuntimeEnvironmentResolver = args.RuntimeEnvironmentResolver;
        }
    }
}