using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2
{
    public static class JwtHelper
    {
        public static Outcome<JwtSecurityToken> ToJwt(this ActorToken token)
        {
            if (!token.IsJwt)
                return Outcome<JwtSecurityToken>.Fail(new FormatException("The token is now a JWT"));

            var jwtHandler = new JwtSecurityTokenHandler();
            if (!jwtHandler.CanReadToken(token.Identity))
                return Outcome<JwtSecurityToken>.Fail(new FormatException("The token is now a JWT"));

            try
            {
                var jwt = jwtHandler.ReadJwtToken(token.Identity);
                return Outcome<JwtSecurityToken>.Success(jwt);
            }
            catch (Exception ex)
            {
                return Outcome<JwtSecurityToken>.Fail(ex);
            }
        }
        
        public static Outcome<JwtSecurityToken> ToJwt(this string token)
        {
            try
            {
                var actorToken = new ActorToken(token);
                return token.ToJwt();
            }
            catch (Exception ex)
            {
                return Outcome<JwtSecurityToken>.Fail(ex);
            }
        }

        public static string GetActorId(this JwtSecurityToken token)
        {
            return token.Subject;
        }
        
        /// <summary>
        ///   Examines a string and returns a value that indicates whether it is a security token that
        ///   represents a system identity (as opposed to an identity supported by an identity provider).
        ///   This is often the case when the client is an autonomous service, such as a web job,
        ///   that was authorized via a Client Credential Grant. 
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the string is a token that represents a system identity; otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="IsSystemIdentityToken(ActorToken)"/>
        public static bool IsSystemIdentityToken(this string stringValue)
        {
            var outcome = stringValue.ToJwt();
            return outcome && isSystemIdentityToken(outcome.Value!);
        }

                /// <summary>
        ///   Examines a token and returns a value that indicates whether the token represents a system identity
        ///   (as opposed to an identity supported by an identity provider). This is often the case when the
        ///   client is an autonomous service, such as a web job, that was authorized via a Client Credential Grant. 
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the token represents a system identity; otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="IsSystemIdentityToken(string)"/>
        public static bool IsSystemIdentityToken(this ActorToken actorToken) 
            => isSystemIdentityToken(actorToken.ToJwtSecurityToken());

        static bool isSystemIdentityToken(JwtSecurityToken? jwt)
        {
            if (jwt is null)
                return false;
                
            const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;
            var userTypeClaim = jwt.Claims.FirstOrDefault(i => i.Type.Equals("userType", Comparison));
            return userTypeClaim is { } && userTypeClaim.Value.Equals("system", Comparison);
        }
    }
}