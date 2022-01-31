using System.Net.Http.Headers;

namespace TetraPak
{
    public static class CredentialsHelper
    {
        public static AuthenticationHeaderValue ToAuthenticationHeaderValue(this BasicAuthCredentials credentials)
        {
            return new AuthenticationHeaderValue("Basic", credentials.Encoded);
        }
    }
}