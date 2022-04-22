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
        public static Credentials? GetClientCredentials(this AuthContext authContext)
        {
            var credentials = authContext.Options.GetClientCredentials();
            if (credentials is { })
                return credentials;

            var clientId = authContext.Configuration?.ClientId;
            return !string.IsNullOrEmpty(clientId)
                ? new Credentials(clientId!, authContext.Configuration?.ClientSecret)
                : null;
        }

        static IAuthInfo? getAuthorityInfo(this AuthContext authContext) => authContext.Options.GetAuthInfo();

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
        public static string? GetAuthorityUri(this AuthContext authContext) =>
            authContext.getAuthorityInfo()?.AuthorityUri;

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
        public static string? GetTokenIssuerUri(this AuthContext authContext) =>
            authContext.getAuthorityInfo()?.TokenIssuerUri;

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
        public static string? GetDeviceCodeIssuerUri(this AuthContext authContext) =>
            authContext.getAuthorityInfo()?.DeviceCodeIssuerUri;

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
        public static string? GetRedirectUri(this AuthContext authContext) =>
            authContext.getAuthorityInfo()?.RedirectUri;
    }
}