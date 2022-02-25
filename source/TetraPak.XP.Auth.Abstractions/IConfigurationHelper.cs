using TetraPak.XP.Configuration;

namespace TetraPak.XP.Auth.Abstractions
{
    public static class IConfigurationHelper
    {
        /// <summary>
        ///   Examines a string and returns a value to indicate whether the value identifies
        ///   an attribute used for auth configuration. This is to ensure there is no risk of confusing
        ///   services or endpoints with such attributes. 
        /// </summary>
        /// <param name="config">
        ///   The extended configuration object.
        /// </param>
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
        // ReSharper disable once UnusedParameter.Global
        public static bool IsAuthIdentifier(this IConfiguration config, string identifier)
        {
            return identifier switch
            {
                nameof(IServiceAuthConfig.AuthorityUri) => true,
                nameof(IServiceAuthConfig.TokenIssuerUri) => true,
                nameof(IServiceAuthConfig.DeviceCodeIssuerUri) => true,
                nameof(IServiceAuthConfig.RedirectUri) => true,
                nameof(IServiceAuthConfig.GrantType) => true,
                nameof(IServiceAuthConfig.ClientId) => true,
                nameof(IServiceAuthConfig.ClientSecret) => true,
                nameof(IServiceAuthConfig.Scope) => true,
                _ => false
            };
        }
        
    }
}