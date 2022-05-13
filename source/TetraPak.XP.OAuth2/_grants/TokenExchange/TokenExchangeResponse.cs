using System;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2.TokenExchange
{
    public sealed class TokenExchangeResponse : GrantResponse
    {
        public static Outcome<TokenExchangeResponse> TryParse(TokenExchangeResponseBody body)
        {
            if (!body.AccessToken.IsUnassigned())
                return Outcome<TokenExchangeResponse>.Fail(
                    new FormatException($"Unexpected response. access_token was unassigned"));

            var stringAccessToken = string.IsNullOrWhiteSpace(body.TokenType)
                ? body.AccessToken!
                : $"{body.TokenType} {body.AccessToken}";
            if (!ActorToken.TryParse(stringAccessToken, out var accessToken))
                return Outcome<TokenExchangeResponse>.Fail(
                    new FormatException($"Failed while parsing access_token: {stringAccessToken}"));

            ActorToken? refreshToken = null;
            if (body.RefreshToken.IsAssigned())
            {
                if (!ActorToken.TryParse(stringAccessToken, out refreshToken))
                    return Outcome<TokenExchangeResponse>.Fail(
                        new FormatException($"Failed while parsing refresh_token: {body.RefreshToken}"));
                
            }
            
            var expiresIn = TimeSpan.Zero;
            if (body.ExpiresIn.IsAssigned())
            {
                if (!int.TryParse(body.ExpiresIn, out var seconds))
                    return Outcome<TokenExchangeResponse>.Fail(
                        new FormatException($"Failed while parsing expires_in: {body.ExpiresIn}"));
                expiresIn = TimeSpan.FromSeconds(seconds);
            }

            GrantScope? scope = null;
            if (body.Scope.IsAssigned() && !GrantScope.TryParse(body.Scope, out scope))
                return Outcome<TokenExchangeResponse>.Fail(
                    new FormatException($"Failed while parsing scope: {body.Scope}"));
                
            return Outcome<TokenExchangeResponse>.Success(
                new TokenExchangeResponse(accessToken!, expiresIn, scope, refreshToken));
        }
        
        TokenExchangeResponse(ActorToken accessToken, TimeSpan expiresIn, GrantScope? scope, ActorToken? refreshToken) 
        : base(accessToken, expiresIn, scope, refreshToken)
        {
        }
    }
}