using System.Net.Http.Headers;

namespace TetraPak.XP.Auth.Abstractions
{
    public static class CredentialsHelper
    {
        public static AuthenticationHeaderValue ToAuthenticationHeaderValue(this BasicAuthCredentials credentials)
        {
            return new AuthenticationHeaderValue("Basic", credentials.Encoded);
        }
    }
}