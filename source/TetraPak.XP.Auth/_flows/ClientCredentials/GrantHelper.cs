using System;
using System.Collections.Generic;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Auth.ClientCredentials
{
    static class ClientCredentialsGrantHelper
    {
        const string KeyPendingCodeVerification = "__pendingVerificaion";
        
        internal static Grant ToGrant(this ClientCredentialsResponse response)
        {
            DateTime? expires = null;
            if (response.ExpiresIn != TimeSpan.Zero)
            {
                expires = DateTime.UtcNow.Add(response.ExpiresIn);
            }
             
            var tokens = new List<TokenInfo>()
            {
                new(response.AccessToken!, TokenRole.AccessToken, expires), 
            };
            if (!string.IsNullOrWhiteSpace(response.RefreshToken))
            {
                tokens.Add(new TokenInfo(response.RefreshToken!, TokenRole.RefreshToken));
            }
            var grant = new Grant(tokens.ToArray());
            return new Grant(tokens.ToArray()) { Scope = response.Scope };
        }
    }
}