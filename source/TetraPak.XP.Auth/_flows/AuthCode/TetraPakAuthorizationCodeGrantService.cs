using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth.AuthCode
{
    class TetraPakAuthorizationCodeGrantService : GrantServiceBase, IAuthorizationCodeGrantService
    {
        const string CacheRepository = CacheRepositories.Tokens.AuthCode;
        
        /// <inheritdoc />
        public async Task<Outcome<Grant>> AcquireTokenAsync(GrantOptions options)
        {
            var messageId = GetMessageId();
            var appCredentialsOutcome = await GetAppCredentialsAsync();
            if (!appCredentialsOutcome)
                return Outcome<Grant>.Fail(appCredentialsOutcome.Exception!);

            var appCredentials = appCredentialsOutcome.Value!;
            var cachedOutcome = IsCachingGrants(options)
                ? await GetCachedResponseAsync(CacheRepository, appCredentials)
                : Outcome<Grant>.Fail("Cached grant not allowed");

            if (cachedOutcome)
                return cachedOutcome;

            await removeFromCacheAsync();
            if (string.IsNullOrEmpty(cachedOutcome.Value.RefreshToken))
                return await GetAccessTokenAsync();
            
// #if DEBUG
//             var simulatedAuth = await AuthSimulator.TryGetSimulatedRenewedAccessTokenAsync( todo Support simulated Auth Code grant
//                 cachedOutcome.Value!.RefreshToken!, 
//                 TetraPakConfig, 
//                 TokenCacheKey);
//             if (simulatedAuth)
//                 return simulatedAuth;
// #endif               

            // access token has expired, try renew from refresh token if available ...
            Log.Debug("---- START - Tetra Pak Refresh Token Flow ----");
            Outcome<Grant> result;
            try
            {
                result = await acquireRenewedAccessTokenAsync(cachedOutcome.Value.RefreshToken!);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Outcome<Grant>.Fail(new Exception($"Could not renew access token: {ex.Message}", ex));
            }
            finally
            {
                Log.Debug("---- END - Tetra Pak Refresh Token Flow ----");
            }

            return result ? result : await GetAccessTokenAsync();
        }
        
        Task removeFromCacheAsync()
        {
            return TokenCache?.AttemptDeleteAsync();
        }
        
        // Outcome<Grant> onAuthorizationDone(Outcome<Grant> authResult)
        // {
        //     if (authResult)
        //     {
        //         Authorized?.Invoke(this, new AuthResultEventArgs(authResult));
        //     }
        //     return authResult;
        // }
        
        async Task<Dictionary<string,string>> buildTokenRequestBodyAsync(string authCode, AuthState authState)
        {
            var sb = new StringBuilder();
            sb.Append("grant_type=authorization_code");
            sb.Append($"&code={authCode}");
            sb.Append($"&client_id={TetraPakConfig.ClientId}");
            sb.Append($"&redirect_uri={Uri.EscapeDataString(TetraPakConfig.GetRedirectUri().AbsoluteUri)}");
            if (authState.Verifier is not null)
                sb.Append($"&code_verifier={authState.Verifier}");

            return sb.ToString();
        }

        async Task<Outcome<string>> buildAuthRequestAsync(AuthState authState, AuthContext authContext)
        {
            var sb = new StringBuilder();
            var authority = await TetraPakConfig.GetTokenIssuerUrlAsync();
            Uri redirectUri = await TetraPakConfig.GetRedirectUriAsync(authContext);
            var clientIdOutcome = await TetraPakConfig.GetClientIdAsync(authContext);
            if (!clientIdOutcome)
                return Outcome<string>.Fail(clientIdOutcome.Exception!);
            
            sb.Append($"{authority.AbsoluteUri}?response_type=code");
            sb.Append($"&redirect_uri={Uri.EscapeDataString(redirectUri.AbsoluteUri)}");
            sb.Append($"&client_id={clientIdOutcome.Value!}");
            sb.Append($"&scope={}");

            
            var scopeOutcome = await TetraPakConfig.GetScopeAsync(authContext);
            
            
            if (scopeOutcome)
            {
                
            }
            
            
            if (Config.IsRequestingUserId)
                Config.AddScope(GrantScope.OpenId);
                
            if (!string.IsNullOrEmpty(Config.Scope))
                sb.Append($"&scope={Config.Scope.UrlEncoded()}");

            // state ...
            if (!authState.IsUsed)
                return sb.ToString();

            sb.Append($"&state={HttpUtility.UrlEncode(authState.State)}");
            if (!authState.IsPKCEUsed)
                return sb.ToString();

            sb.Append($"&code_challenge={authState.CodeChallenge}");
            sb.Append($"&code_challenge_method={authState.CodeChallengeMethod}");
            return sb.ToString();
        }

        public TetraPakAuthorizationCodeGrantService(
            ITetraPakConfiguration tetraPakConfig, 
            IHttpClientProvider httpClientProvider,
            ITimeLimitedRepositories? cache = null,
            ITokenCache? tokenCache = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(tetraPakConfig, httpClientProvider, cache, tokenCache, log, httpContextAccessor)
        {
        }
    }
}