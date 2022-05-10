using System.Threading.Tasks;

namespace TetraPak.XP.Auth.Abstractions
{

    /// <summary>
    ///   Provides convenient helper methods for working with <see cref="AuthContext"/>.
    /// </summary>
    public static class AuthContextHelper
    {
        /// <summary>
        ///   Gets client credentials from the <see cref="AuthContext"/>.
        /// </summary>
        /// <param name="authContext">
        ///   The (extended) <see cref="AuthContext"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="Credentials"/> object if client credentials can be resolved from the <see cref="AuthContext"/>;
        ///   otherwise <c>null</c>. 
        /// </returns>
        public static async Task<Credentials?> GetClientCredentialsAsync(this AuthContext authContext)
        {
            var credentials = await authContext.Options.GetClientCredentials();
            if (credentials is { })
                return credentials;

            var clientId = authContext.Configuration?.ClientId;
            return !string.IsNullOrEmpty(clientId)
                ? new Credentials(clientId!, authContext.Configuration?.ClientSecret)
                : null;
        }

        static Task<IAuthInfo?> getAuthorityInfoAsync(this AuthContext authContext) => authContext.Options.GetAuthInfoAsync();

        /// <summary>
        ///   Gets the authority URI from the <see cref="AuthContext"/>.
        /// </summary>
        /// <param name="authContext">
        ///   The (extended) <see cref="AuthContext"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> representation of the URI if successfully resolved from the <see cref="AuthContext"/>;
        ///   otherwise <c>null</c>. 
        /// </returns>
        public static async Task<string?> GetAuthorityUriAsync(this AuthContext authContext)
        {
            var info = await authContext.getAuthorityInfoAsync();
            return info?.AuthorityUri;
        }

        /// <summary>
        ///   Gets the token issuer URI from the <see cref="AuthContext"/>.
        /// </summary>
        /// <param name="authContext">
        ///   The (extended) <see cref="AuthContext"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> representation of the URI if successfully resolved from the <see cref="AuthContext"/>;
        ///   otherwise <c>null</c>. 
        /// </returns>
        public static async Task<string?> GetTokenIssuerUriAsync(this AuthContext authContext)
        {
            var info = await authContext.getAuthorityInfoAsync();
            return info?.TokenIssuerUri;
        }

        /// <summary>
        ///   Gets the device code issuer service URI from the <see cref="AuthContext"/>.
        /// </summary>
        /// <param name="authContext">
        ///   The (extended) <see cref="AuthContext"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> representation of the URI if successfully resolved from the <see cref="AuthContext"/>;
        ///   otherwise <c>null</c>. 
        /// </returns>
        public static async Task<string?> GetDeviceCodeIssuerUri(this AuthContext authContext)
        {
            var info = await authContext.getAuthorityInfoAsync();
            return info?.DeviceCodeIssuerUri;
        }

        /// <summary>
        ///   Gets a redirect URI from the <see cref="AuthContext"/>.
        /// </summary>
        /// <param name="authContext">
        ///   The (extended) <see cref="AuthContext"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> representation of the URI if successfully resolved from the <see cref="AuthContext"/>;
        ///   otherwise <c>null</c>. 
        /// </returns>
        public static async Task<string?> GetRedirectUriAsync(this AuthContext authContext)
        {
            var info = await authContext.getAuthorityInfoAsync();
            return info?.RedirectUri;
        }
    }
}