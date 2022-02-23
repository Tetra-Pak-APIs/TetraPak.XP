using System;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Configuration;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Classes implementing this contract can provide information needed for authorization purposes. 
    /// </summary>
    public interface IServiceAuthConfig : IConfigurationSection
    {
        string TokenIssuerUrl { get; }
        
        string DeviceCodeIssuerUrl { get; }
        
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
        Task<Outcome<string>> GetClientIdAsync(AuthContext authContext);
        
        /// <summary>
        ///   Gets a client secret.
        /// </summary>
        /// <param name="authContext">
        ///   Details the auth context in which the (confidential) client secrets are requested.
        /// </param>
        Task<Outcome<string>> GetClientSecretAsync(AuthContext authContext);
        
        /// <summary>
        ///   Gets a redirect URI.
        /// </summary>
        /// <param name="authContext">
        ///   Details the auth context in which the redirect URI is requested.
        /// </param>
        Task<Outcome<Uri>> GetRedirectUriAsync(AuthContext authContext);

        /// <summary>
        ///   Gets a scope to be requested for authorization while, optionally, specifying a default scope.
        /// </summary>
        /// <param name="authContext">
        ///   Details the auth context in which the grant is requested.
        /// </param>
        /// <param name="useDefault">
        ///   (optional)<br/>
        ///   Specifies a default value to be returned if scope cannot be resolved.
        /// </param>
        /// <param name="cancellationToken">
        ///   (optional)<br/>
        ///   Cancellation token for cancellation the operation.
        /// </param>
        Task<Outcome<GrantScope>> GetScopeAsync(
            AuthContext authContext,
            MultiStringValue? useDefault = null,
            CancellationToken? cancellationToken = null);

        /// <summary>
        ///   Gets a value specifying whether state is to be used in the ongoing authorization context. 
        /// </summary>
        /// <param name="authContext">
        ///   Details the auth context in which the state is needed.
        /// </param>
        /// <param name="useDefault">
        ///   (optional)<br/>
        ///   Specifies a default value to be returned if the value cannot be resolved.
        /// </param>
        /// <param name="cancellationToken">
        ///   (optional)<br/>
        ///   Cancellation token for cancellation the operation.
        /// </param>
        /// <returns>
        ///   <c>true</c> if state is to be used; otherwise <c>false</c>.
        /// </returns>
        Task<Outcome<bool>> IsStateUsedAsync(
            AuthContext authContext, 
            bool useDefault,
            CancellationToken? cancellationToken = null);
        
        /// <summary>
        ///   Gets a value specifying whether a PKCE is to be used in the ongoing authorization context. 
        /// </summary>
        /// <param name="authContext">
        ///   Details the auth context in which the PKCE is needed.
        /// </param>
        /// <param name="useDefault">
        ///   (optional)<br/>
        ///   Specifies a default value to be returned if the value cannot be resolved.
        /// </param>
        /// <param name="cancellationToken">
        ///   (optional)<br/>
        ///   Cancellation token for cancellation the operation.
        /// </param>
        /// <returns>
        ///   <c>true</c> if PKCE is to be used; otherwise <c>false</c>.
        /// </returns>
        Task<Outcome<bool>> IsPkceUsedAsync(
            AuthContext authContext, 
            bool useDefault,
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

        // /// <summary>
        // ///   Gets the configuration path. obsolete
        // /// </summary>
        // ConfigPath? ConfigPath { get; }

        Task<Outcome<Uri>> GetTokenIssuerUrlAsync(AuthContext authContext);
        
        Task<Outcome<Uri>> GetAuthorityUrlAsync(AuthContext authContext);
        
        Task<Outcome<Uri>> GetDeviceCodeIssuerUrlAsync(AuthContext authContext);
        
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