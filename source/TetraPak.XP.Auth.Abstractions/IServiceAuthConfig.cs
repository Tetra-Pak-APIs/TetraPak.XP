using TetraPak.XP.Configuration;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Classes implementing this contract can provide information needed for authorization purposes. 
    /// </summary>
    public interface IServiceAuthConfig : IConfigurationSection
    {
        string AuthDomain { get; }
        
        public string? AuthorityUri { get; }
        
        string? TokenIssuerUri { get; }
        
        string? DeviceCodeIssuerUri { get; }

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
        GrantScope? Scope { get; }

        /// <summary>
        ///   Specifies the grant type (a.k.a. OAuth "flow") used at this configuration level.
        /// </summary>
        /// <exception cref="HttpServerConfigurationException">
        ///   The configured (textual) value could not be parsed as a <see cref="GrantType"/> (enum) value. 
        /// </exception>
        GrantType GrantType { get; }

        /// <summary>
        ///   Gets a "raw" configured value, as it is specified within the <see cref="IConfiguration"/> sources,
        ///   unaffected by delegates or other (internal) logic.
        /// </summary>
        /// <param name="key">
        ///   Identifies the requested value.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> when a value is configured; otherwise <c>null</c>.
        /// </returns>
        string? GetRawConfiguredValue(string key);
        
        bool UseState { get; }
        
        bool UsePKCE { get; }
    }
}