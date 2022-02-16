using System;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Auth.DeviceCode
{
    public class DeviceCodeResponse : GrantResponse
    {
        internal bool IsPendingVerification { get; private set; }

        internal static Outcome<DeviceCodeResponse> TryParse(DeviceCodeAuthorizationResponseBody body)
        {
            var accessToken = string.IsNullOrWhiteSpace(body.TokenType)
                ? body.AccessToken
                : $"{body.TokenType} {body.AccessToken}";
            if (!ActorToken.TryParse(accessToken, out var actorToken))
                return Outcome<DeviceCodeResponse>.Fail(
                    new FormatException($"Failed while parsing access_token: {accessToken}"));

            var expiresIn = TimeSpan.Zero;
            if (!string.IsNullOrWhiteSpace(body.ExpiresIn))
            {
                if (!int.TryParse(body.ExpiresIn, out var seconds))
                    return Outcome<DeviceCodeResponse>.Fail(
                        new FormatException($"Failed while parsing expires_in: {body.ExpiresIn}"));
                expiresIn = TimeSpan.FromSeconds(seconds);
            }

            if (string.IsNullOrWhiteSpace(body.Scope))
                return Outcome<DeviceCodeResponse>.Success(
                    new DeviceCodeResponse(accessToken!, expiresIn, null));

            if (!MultiStringValue.TryParse<MultiStringValue>(body.Scope, out var scope))
                return Outcome<DeviceCodeResponse>.Fail(
                    new FormatException($"Failed while parsing expires_in: {body.ExpiresIn}"));

            return Outcome<DeviceCodeResponse>.Success(
#if NET5_0_OR_GREATER                
                new DeviceCodeResponse(actorToken, expiresIn, scope));
#else
                new DeviceCodeResponse(actorToken!, expiresIn, scope));
#endif
        }

        internal static DeviceCodeResponse ForPendingCodeVerification(bool value) => new() { IsPendingVerification = value };

        DeviceCodeResponse() : base(null!, TimeSpan.Zero, null)
        {
        }
        
        DeviceCodeResponse(ActorToken accessToken, TimeSpan expiresIn, MultiStringValue? scope)
        : base(accessToken, expiresIn, scope)
        {
        }

    }
}