using System;
using System.Collections.Generic;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2.DeviceCode
{
    static class DeviceCodeGrantHelper
    {
        const string KeyPendingCodeVerification = "__pendingVerificaion";
        
        internal static Grant ForPendingCodeVerification(this Grant self)
        {
            self.SetFlag(KeyPendingCodeVerification);
            return self;
        }

        internal static bool IsPendingVerification(this Grant self) => self.IsFlagSet(KeyPendingCodeVerification);

        internal static Outcome<Grant> ToGrant(this DeviceCodePollVerificationResponseBody pollVerificationResponse)
        {
            DateTime? expires = null;
            if (!string.IsNullOrEmpty(pollVerificationResponse.ExpiresIn))
            {
                if (!double.TryParse(pollVerificationResponse.ExpiresIn, out var dValue))
                    return Outcome<Grant>.Fail(
                        new FormatException($"Unexpected response value for '{DeviceCodePollVerificationResponseBody.KeyExpiresIn}': \"{pollVerificationResponse.ExpiresIn}\""));
                
                expires = DateTime.UtcNow.Add(TimeSpan.FromSeconds(dValue));
            }
             
            var tokens = new List<TokenInfo>()
            {
                new(pollVerificationResponse.AccessToken!, TokenRole.AccessToken, expires), 
            };
            if (!string.IsNullOrWhiteSpace(pollVerificationResponse.RefreshToken))
            {
                tokens.Add(new TokenInfo(pollVerificationResponse.RefreshToken!, TokenRole.RefreshToken));
            }
            var grant = new Grant(tokens.ToArray());
            return Outcome<Grant>.Success(new Grant(tokens.ToArray()) { Scope = pollVerificationResponse.Scope });
        }
    }
}