using System;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.OAuth2.ClientCredentials
{
    public class ClientCredentialsResponse : GrantResponse
    {
        internal static Outcome<ClientCredentialsResponse> TryParse(ClientCredentialsResponseBody body)
        {
            var accessToken = string.IsNullOrWhiteSpace(body.TokenType)
                ? body.AccessToken
                : $"{body.TokenType} {body.AccessToken}";
            if (!ActorToken.TryParse(accessToken, out var actorToken))
                return Outcome<ClientCredentialsResponse>.Fail(
                    new FormatException($"Failed while parsing access_token: {accessToken}"));

            var expiresIn = TimeSpan.Zero;
            if (!string.IsNullOrWhiteSpace(body.ExpiresIn))
            {
                if (!int.TryParse(body.ExpiresIn, out var seconds))
                    return Outcome<ClientCredentialsResponse>.Fail(
                        new FormatException($"Failed while parsing expires_in: {body.ExpiresIn}"));
                expiresIn = TimeSpan.FromSeconds(seconds);
            }

            if (string.IsNullOrWhiteSpace(body.Scope))
                return Outcome<ClientCredentialsResponse>.Success(
                    new ClientCredentialsResponse(accessToken!, expiresIn, null));

            if (!MultiStringValue.TryParse<MultiStringValue>(body.Scope, out var scope))
                return Outcome<ClientCredentialsResponse>.Fail(
                    new FormatException($"Failed while parsing expires_in: {body.ExpiresIn}"));

            return Outcome<ClientCredentialsResponse>.Success(new ClientCredentialsResponse(
                actorToken, expiresIn, scope));
        }
        
        ClientCredentialsResponse(ActorToken accessToken, TimeSpan expiresIn, MultiStringValue? scope)
        : base(accessToken, expiresIn, scope, null)
        {
        }
    }
}