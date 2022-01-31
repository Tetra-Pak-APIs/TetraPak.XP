using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.IdentityModel.Tokens;

namespace TetraPak.XP.Auth.OIDC
{
    public class JwtTokenValidationOptions
    {
        public bool ValidateLifetime { get; set; }

        public bool ValidateIssuerSigningKey { get; set; }

        public bool ValidateIssuer { get; set; }

        public bool ValidateAudience { get; set; }

        public bool ValidateActor { get; set; }

        public bool SaveSigninToken { get; set; }

        public bool RequireSignedTokens { get; set; }

        public bool RequireExpirationTime { get; set; }

        internal TokenValidationParameters ToTokenValidationParameters(
            JwtSecurityToken jwtSecurityToken,
            DiscoveryDocument discoveryDocumentValue, 
            JsonWebKeySet jwksKeySet)
        {
            return new TokenValidationParameters
            {
                ValidIssuer = discoveryDocumentValue.Issuer,
                ValidAudience = jwtSecurityToken.Audiences.First(),
                IssuerSigningKeys = jwksKeySet.Keys,
                NameClaimType = "name",
                RoleClaimType = "role",
                ValidateAudience = ValidateAudience,
                RequireExpirationTime = RequireExpirationTime,
                RequireSignedTokens = RequireSignedTokens,
                SaveSigninToken = SaveSigninToken,
                ValidateActor = ValidateActor,
                ValidateIssuer = ValidateIssuer,
                ValidateIssuerSigningKey = ValidateIssuerSigningKey,
                ValidateLifetime = ValidateLifetime
            };
        }

        public JwtTokenValidationOptions() : this(new TokenValidationParameters())
        {
        }

        public JwtTokenValidationOptions(TokenValidationParameters from)
        {
            RequireExpirationTime = from.RequireExpirationTime;
            RequireSignedTokens = from.RequireSignedTokens;
            SaveSigninToken = from.SaveSigninToken;
            ValidateActor = from.ValidateActor;
            ValidateAudience = from.ValidateAudience;
            ValidateIssuer = from.ValidateIssuer;
            ValidateIssuerSigningKey = from.ValidateIssuerSigningKey;
            ValidateLifetime = from.ValidateLifetime;
        }
    }
}