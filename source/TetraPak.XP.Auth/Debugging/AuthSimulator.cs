#if DEBUG
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using TetraPak.XP.Caching;

namespace TetraPak.XP.Auth.Debugging
{
    // todo maybe(?) consider improving the Auth simulator to truly simulate the actual flow, including flipping to an "external web page"
    // todo consider improving the the Auth simulator to also support retrieving user information
    public class AuthSimulator
    {
        public static bool IsSimulating
        {
            get
            {
#if SIMULATED_AUTH
                    return true;
#else
                    return false;
#endif
            }
        }

        public static TimeSpan AccessTokenLongevity { get; set; }

        public static TimeSpan IdTokenLongevity { get; set; }

        public static async Task<Outcome<Grant>> TryGetSimulatedAccessTokenAsync(AuthConfig config, string cacheKey)
        {
            if (!IsSimulating)
                return Outcome<Grant>.Fail(new Exception("Not simulating"));

            return await GetAccessTokenAsync(config, cacheKey);
        }

        public static async Task<Outcome<Grant>> GetAccessTokenAsync(AuthConfig config, string cacheKey)
        {
            throw new NotImplementedException(); // nisse
            // var accessToken = new TokenInfo(new RandomString(), TokenRole.AccessToken, DateTime.Now.Add(AccessTokenLongevity), null);
            // var refreshToken = new TokenInfo(new RandomString(), TokenRole.RefreshToken, null, null);
            // if (!config.IsRequestingUserId)
            //     return await config.CacheAsync(new AuthResult(null!, null!, accessToken, refreshToken), cacheKey);
            //
            // var idToken = new TokenInfo(simulatedJwtToken(), TokenRole.IdToken, DateTime.Now.Add(IdTokenLongevity), onValidateSimulatedIdToken);
            // return await config.CacheAsync(new AuthResult(null!, null!, accessToken, refreshToken, idToken), cacheKey);
        }

        public static async Task<Outcome<Grant>> TryGetSimulatedRenewedAccessTokenAsync(string refreshToken, AuthConfig config, string cacheKey)
        {
            throw new NotImplementedException(); // nisse
            // var canBeRefreshed = await config.TryGetFromRefreshTokenAsync(refreshToken); 
            // if (!canBeRefreshed)
            //     return Outcome<AuthResult>.Fail(new Exception("Invalid refresh token"));
            //     
            // var accessToken = new TokenInfo(new RandomString(), TokenRole.AccessToken, DateTime.Now.Add(AccessTokenLongevity), null);
            // return await config.CacheAsync(new AuthResult(null, null, accessToken), cacheKey);
        }

        static string simulatedJwtToken()
        {
            var key = new RandomString();
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("name", "Simulated-Client"),
                new Claim("email", "simulated@client.net")
            };
            var jwtSecurityToken = new JwtSecurityToken(
                typeof(AuthSimulator).FullName,
                Assembly.GetEntryAssembly()?.FullName ?? "simulated-client",
                claims,
                null,
                DateTime.Now.Add(IdTokenLongevity),
                credentials);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.WriteToken(jwtSecurityToken);
            return jwtToken;
        }

        static Task<Outcome<string>> onValidateSimulatedIdToken(string token) => Task.FromResult(Outcome<string>.Success(token));

        static AuthSimulator()
        {
            AccessTokenLongevity = TimeSpan.FromMinutes(5);
            IdTokenLongevity = AccessTokenLongevity;
        }
    }

    static class AuthConfigExtensions
    {
        static readonly Dictionary<string,Grant> s_authResults = new Dictionary<string, Grant>();

        public static async Task<Outcome<Grant>> CacheAsync(this AuthConfig config, Grant grant, string cacheKey)
        {
            if (config.IsCaching)
            {
                // await config.TokenCache.AddAsync(cacheKey, authResult, true);
                await config.TokenCache.AttemptCreateOrUpdateAsync(grant, cacheKey);
            }

            var refreshToken = grant.Tokens?.FirstOrDefault(i => i.Role == TokenRole.RefreshToken);
            if (refreshToken != null)
                s_authResults.Add(refreshToken.TokenValue, grant);
            
            return Outcome<Grant>.Success(grant);
        }
        
        public static Task<Outcome<Grant>> TryGetFromRefreshTokenAsync(this AuthConfig config, string refreshToken)
        {
            if (!s_authResults.TryGetValue(refreshToken, out var authResult))
                return Task.FromResult(Outcome<Grant>.Fail(new Exception("No refresh token")));
            
            s_authResults.Remove(refreshToken);
            return Task.FromResult(Outcome<Grant>.Success(authResult));
        }
    }
}
#endif