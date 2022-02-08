using System;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Configuration;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Classes implementing this contract can provide information needed for authorization purposes. 
    /// </summary>
    public interface IServiceAuthConfig 
    {
        /// <summary>
        ///   Specifies the grant type (a.k.a. OAuth "flow") used at this configuration level.
        /// </summary>
        /// <exception cref="HttpServerConfigurationException">
        ///   The configured (textual) value could not be parsed into a <see cref="GrantType"/> (enum) value. 
        /// </exception>
        GrantType GrantType { get; }
        
        /// <summary>
        ///   Gets a configured client id at this configuration level.
        /// </summary>
        /// <see cref="GetClientIdAsync"/>
        string? ClientId { get; }
        
        /// <summary>
        ///   Gets a configured client secret at this configuration level.
        /// </summary>
        string? ClientSecret { get; }

        /// <summary>
        ///   Gets an authorization scope at this configuration level.
        /// </summary>
        MultiStringValue? Scope { get; }

        /// <summary>
        ///   Gets a client id.
        /// </summary>
        /// <param name="authContext">
        ///     Details the auth context in which the (confidential) client secrets are requested.
        /// </param>
        /// <param name="cancellationToken">
        ///   (optional)<br/>
        ///   Cancellation token for cancellation the operation.
        /// </param>
        Task<Outcome<string>> GetClientIdAsync(AuthContext authContext, CancellationToken? cancellationToken = null);
        
        /// <summary>
        ///   Gets a client secret.
        /// </summary>
        /// <param name="authContext">
        ///   Details the auth context in which the (confidential) client secrets are requested.
        /// </param>
        /// <param name="cancellationToken">
        ///   (optional)<br/>
        ///   Cancellation token for cancellation the operation.
        /// </param>
        Task<Outcome<string>> GetClientSecretAsync(AuthContext authContext, CancellationToken? cancellationToken = null);

        /// <summary>
        ///   Gets a scope to be requested for authorization while, optionally, specifying a default scope.
        /// </summary>
        /// <param name="authContext">
        ///   Details the auth context in which the (confidential) client secrets are requested.
        /// </param>
        /// <param name="useDefault">
        ///   (optional)<br/>
        ///   Specifies a default value to be returned if scope cannot be resolved.
        /// </param>
        /// <param name="cancellationToken">
        ///   (optional)<br/>
        ///   Cancellation token for cancellation the operation.
        /// </param>
        Task<Outcome<MultiStringValue>> GetScopeAsync(
            AuthContext authContext,
            MultiStringValue? useDefault = null,
            CancellationToken? cancellationToken = null);

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
        string? GetConfiguredValue(string key);

        /// <summary>
        ///   Gets the configuration path.
        /// </summary>
        ConfigPath? ConfigPath { get; }

        Task<Uri> GetTokenIssuerUrlAsync();
        
        // /// <summary>
        // ///   Gets the <see cref="IConfiguration"/> instance used to populate the properties. obsolete
        // /// </summary>
        // IConfiguration Configuration { get; }
        
        // /// <summary>
        // ///   Gets an <see cref="AmbientData"/> object.
        // /// </summary>
        // AmbientData AmbientData { get; }
        //
        // /// <summary>
        // ///   Gets a declaring configuration (when this configuration is a sub configuration).
        // /// </summary>
        // IServiceAuthConfig? ParentConfig { get; }
                
        /// <summary>
        ///   Examines a string and returns a value to indicate whether the value identifies
        ///   an attribute used for auth configuration. This is to ensure there is no risk of confusing
        ///   services or endpoints with such attributes. 
        /// </summary>
        /// <param name="identifier">
        ///   The identifier being examined.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="identifier"/> matches an auth configuration attribute; otherwise <c>false</c>. 
        /// </returns>
        /// <remarks>
        ///   Examples of auth identifiers: "<c>ConfigPath</c>", "<c>GrantType</c>",
        ///   "<c>ClientId</c>", "<c>ClientSecret</c>", "<c>Scope</c>".
        /// </remarks>
        bool IsAuthIdentifier(string identifier);
        
    }
}