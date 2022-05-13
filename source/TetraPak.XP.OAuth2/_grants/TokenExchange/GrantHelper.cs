using System;
using System.Collections.Generic;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2.TokenExchange
{
    static class TokenExchangeGrantHelper
    {
        internal static Grant ToGrant(this TokenExchangeResponse response)
        {
            DateTime? expires = null;
            if (response.ExpiresIn != TimeSpan.Zero)
            {
                expires = XpDateTime.UtcNow.Add(response.ExpiresIn);
            }
             
            var tokens = new List<TokenInfo>()
            {
                new(response.AccessToken, TokenRole.AccessToken, expires), 
            };
            if (!string.IsNullOrWhiteSpace(response.RefreshToken))
            {
                tokens.Add(new TokenInfo(response.RefreshToken!, TokenRole.RefreshToken));
            }

            return new Grant(tokens.ToArray()) { Scope = response.Scope };
        }
    }
}