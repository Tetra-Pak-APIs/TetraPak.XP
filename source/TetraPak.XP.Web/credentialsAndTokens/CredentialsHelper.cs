using System.Net.Http.Headers;
using TetraPak.XP.Web.credentialsAndTokens;

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