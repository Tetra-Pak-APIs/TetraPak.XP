using System.Net.Http.Headers;

namespace TetraPak.XP.Auth.Abstractions
{
    public static class CredentialsHelper
    {
        /// <summary>
        ///   Constructs and returns a <see cref="AuthenticationHeaderValue"/> from <see cref="Credentials"/>.
        /// </summary>
        /// <param name="credentials">
        ///   The <see cref="Credentials"/> to be used for the <see cref="ToAuthenticationHeaderValue"/> result.
        /// </param>
        /// <returns>
        ///   A <see cref="AuthenticationHeaderValue"/> object.
        /// </returns>
        public static AuthenticationHeaderValue ToAuthenticationHeaderValue(this Credentials credentials)
        {
            return credentials is BasicAuthCredentials basicAuthCredentials
                ? new AuthenticationHeaderValue(BasicAuthCredentials.Scheme.Trim(), basicAuthCredentials.Encoded)
                : new AuthenticationHeaderValue(string.Empty, credentials.Identity);
        }
    }
}