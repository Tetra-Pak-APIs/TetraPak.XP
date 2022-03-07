

using Microsoft.Extensions.Configuration;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Classes implementing this contract can provide information needed for authorization purposes. 
    /// </summary>
    public interface IAuthConfiguration : IConfigurationSection
    {
        /// <summary>
        ///   Gets the <see cref="RuntimeEnvironment"/> for the running host.
        /// </summary>
        RuntimeEnvironment RuntimeEnvironment { get; }
        
        /// <summary>
        ///   Gets the authority domain to be used.
        /// </summary>
        string AuthDomain { get; }
        
        /// <summary>
        ///   Gets the authority endpoint to be used (eg. for Auth Code/OIDC grants).
        /// </summary>
        string AuthorityUri { get; }
        
        /// <summary>
        ///   Gets the token issuer endpoint to be used (eg. for Auth Code/OIDC grants).
        /// </summary>
        string TokenIssuerUri { get; }

        /// <summary>
        ///   Gets the endpoint to be used for obtaining a code for the OAuth Device Code grant.
        /// </summary>
        string DeviceCodeIssuerUri { get; }

        /// <summary>
        ///   Gets the redirect URI to be used (eg. for Auth Code/OIDC grants).
        /// </summary>
        public string? RedirectUri { get; }

        /// <summary>
        ///   Gets a configured client id at this configuration level.
        /// </summary>
        string? ClientId { get; }
        
        /// <summary>
        ///   Gets a configured client secret at this configuration level.
        /// </summary>
        string? ClientSecret { get; }
        
        /// <summary>
        ///   Gets an authorization scope at this configuration level.
        /// </summary>
        GrantScope? OidcScope { get; }

        /// <summary>
        ///   Specifies the grant type (a.k.a. OAuth "flow") used at this configuration level.
        /// </summary>
        /// <exception cref="HttpServerConfigurationException">
        ///   The configured (textual) value could not be parsed as a <see cref="GrantType"/> (enum) value. 
        /// </exception>
        GrantType GrantType { get; }

        /// <summary>
        ///   Gets a flag indicating whether state is to be supported during a grant request (eg. Auth Code/OIDC grants)
        /// </summary>
        bool OidcState { get; }
        
        /// <summary>
        ///   Gets a flag indicating whether a PKCE value is to be supported during a grant request (eg. Auth Code/OIDC grants)
        /// </summary>
        bool OidcPkce { get; }
        
        /// <summary>
        ///   Gets a value indicating whether the solution enables caching (please see remarks).
        /// </summary>
        /// <remarks>
        ///   Please note that caching can be configured for different purposes but this might be a handy
        ///   setting to quickly kill all caching. Enabling caching simply means more detailed caching
        ///   configuration is honored.
        /// </remarks>
        bool IsCaching { get; }
    }
}