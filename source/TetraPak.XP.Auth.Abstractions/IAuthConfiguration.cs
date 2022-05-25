

using System;
using Microsoft.Extensions.Configuration;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Classes implementing this contract can provide information needed for authorization purposes. 
    /// </summary>
    public interface IAuthConfiguration : IConfigurationSection, IAuthInfo
    {
        /// <summary>
        ///   Gets the <see cref="RuntimeEnvironment"/> for the running host.
        /// </summary>
        RuntimeEnvironment RuntimeEnvironment { get; }
        
        /// <summary>
        ///   Gets the preferred browser experience (see <see cref="BrowserExperience"/>).
        /// </summary>
        BrowserExperience BrowserExperience { get; }
        
        /// <summary>
        ///   Gets the authority domain to be used.
        /// </summary>
        string AuthDomain { get; }
        
        /// <summary>
        ///   Gets a configured client id at this configuration level.
        /// </summary>
        string? ClientId { get; }
        
        /// <summary>
        ///   Gets a configured client secret at this configuration level.
        /// </summary>
        string? ClientSecret { get; }
        
        /// <summary>
        ///   Specifies the grant type (a.k.a. OAuth "flow") used at this configuration level.
        /// </summary>
        /// <exception cref="HttpServerConfigurationException">
        ///   The configured (textual) value could not be parsed as a <see cref="GrantType"/> (enum) value. 
        /// </exception>
        GrantType GrantType { get; }
        
        /// <summary>
        ///   Gets a value indicating whether the solution enables caching (please see remarks).
        /// </summary>
        /// <remarks>
        ///   Please note that caching can be configured for different purposes but this might be a handy
        ///   setting to quickly kill all caching. Enabling caching simply means more detailed caching
        ///   configuration is honored.
        /// </remarks>
        bool IsCaching { get; }
        
        /// <summary>
        ///   Gets a value specifying a maximum allowed time for the affected operation to complete, or be cancelled.
        /// </summary>
        TimeSpan? Timeout { get; }
    }
}