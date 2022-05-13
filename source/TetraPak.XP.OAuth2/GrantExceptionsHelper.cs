using TetraPak.XP.Documentation;

namespace TetraPak.XP.OAuth2
{
    public static class GrantExceptionsHelper
    {
        public static HttpServerException TokenExchangeNotSupportedForSystemIdentity(this IGrantService _)
        {
            return HttpServerException.BadRequest(
                "Token exchange is not supported for system identities"+
                Docs.PleaseSee(Docs.DevPortal.TokenExchangeSubjectTokenTypes));
        }
    }
}