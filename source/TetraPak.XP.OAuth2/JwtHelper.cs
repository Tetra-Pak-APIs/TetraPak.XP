using System.IdentityModel.Tokens.Jwt;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2
{
    public static class JwtHelper
    {
        public static JwtSecurityToken? ToJwt(this ActorToken token)
        {
            if (!token.IsJwt)
                return null;

            var jwtHandler = new JwtSecurityTokenHandler();
            return jwtHandler.CanReadToken(token.Identity) 
                ? jwtHandler.ReadJwtToken(token.Identity) 
                : null;
        }

        public static string GetActorId(this JwtSecurityToken token)
        {
            return token.Subject;
        }
    }
}