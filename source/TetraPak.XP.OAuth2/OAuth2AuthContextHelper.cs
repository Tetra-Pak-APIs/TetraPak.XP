using System;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2
{
    public static class OAuth2AuthContextHelper
    {
        public static string GetGrantCacheRepository(this AuthContext context)
        {
            return context.GrantType switch
            {
                GrantType.TX => CacheRepositories.Tokens.TokenExchange,
                GrantType.CC => CacheRepositories.Tokens.ClientCredentials,
                GrantType.AC => CacheRepositories.Tokens.OIDC,
                GrantType.OIDC => CacheRepositories.Tokens.OIDC,
                GrantType.DC => CacheRepositories.Tokens.DeviceCode,
                GrantType.RF => CacheRepositories.Tokens.Refresh,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public static string GetRefreshTokenCacheRepository(this AuthContext context)
        {
            return $"{context.GetGrantCacheRepository()}_{CacheRepositories.Tokens.Refresh}";
        }

    }
}