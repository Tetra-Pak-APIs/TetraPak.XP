using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.Auth.Debugging;
using TetraPak.XP.Caching;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging;
using TetraPak.XP.OAuth2;
using TetraPak.XP.Web;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;
#if DEBUG
#endif

[assembly: XpService(typeof(TetraPakAuthenticator))]
[assembly: XpService(typeof(TetraPakAuthCallbackHandler))]

namespace TetraPak.XP.OAuth2
{
    /// <summary>
    ///   A custom (Tetra Pak) implementation of the <see cref="IAuthenticator"/> contract.
    /// </summary>
    public class TetraPakAuthenticator : AbstractAuthenticator
    {
        internal static event EventHandler<AuthResultEventArgs>? Authorized;

        /// <inheritdoc />
        public override async Task<Outcome<Grant>> GetAccessTokenAsync(
            bool allowCached = true, 
            CancellationTokenSource? cancellationTokenSource = null)
        {
            if (allowCached)
            {
                var cachedOutcome = await Config.GetCachedTokenAsync(CacheKey); 
                if (cachedOutcome && cachedOutcome.Value!.AccessToken is {})
                    return onAuthorizationDone(cachedOutcome);
            }
            
#if DEBUG
            var simulatedAuth = await AuthSimulator.TryGetSimulatedAccessTokenAsync(Config, CacheKey);
            if (simulatedAuth)
                return simulatedAuth;
#endif
            try
            {
                LogDebug("---- START - Tetra Pak Code Grant Flow ----");
                return await acquireTokenViaWebUIAsync();
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message);
                LogDebug("---- END - Tetra Pak Code Grant Flow ----");
                return Outcome<Grant>.Fail(new Exception("Could not acquire an access token", ex));
            }
        }

        /// <inheritdoc />
        public override async Task<Outcome<Grant>> GetAccessTokenSilentlyAsync(
            CancellationTokenSource? cancellationTokenSource = null)
        {
            if (!IsCaching)
                return await GetAccessTokenAsync(false, cancellationTokenSource);

            var ct = cancellationTokenSource?.Token ?? CancellationToken.None;
            var cachedOutcome = await Config.GetCachedTokenAsync(CacheKey);
            if (!cachedOutcome)
                return await GetAccessTokenAsync(false, cancellationTokenSource);

            if (cachedOutcome.Value!.AccessToken is {})
            {
                if (!cachedOutcome.Value.Expires.HasValue || DateTime.Now < cachedOutcome.Value.Expires.Value)
                    return onAuthorizationDone(cachedOutcome);
            }

            await removeFromCacheAsync();
            if (string.IsNullOrEmpty(cachedOutcome.Value.RefreshToken))
                return await GetAccessTokenAsync();
            
// #if DEBUG  
//             var simulatedAuth = await AuthSimulator.TryGetSimulatedRenewedAccessTokenAsync(cachedOutcome.Value!.RefreshToken!, Config, CacheKey);
//             if (simulatedAuth)
//                 return simulatedAuth;
// #endif               

            // access token has expired, try renew from refresh token if available ...
            LogDebug("---- START - Tetra Pak Refresh Token Flow ----");
            Outcome<Grant> result;
            try
            {
                result = await acquireRenewedAccessTokenAsync(cachedOutcome.Value.RefreshToken!);
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message);
                return Outcome<Grant>.Fail(new Exception("Could not renew access token", ex));
            }
            finally
            {
                LogDebug("---- END - Tetra Pak Refresh Token Flow ----");
            }

            return result ? result : await GetAccessTokenAsync();
        }

        Outcome<Grant> onAuthorizationDone(Outcome<Grant> authResult)
        {
            if (authResult)
            {
                Authorized?.Invoke(this, new AuthResultEventArgs(authResult));
            }
            return authResult;
        }

        async Task<Outcome<Grant>> acquireTokenViaWebUIAsync()
        {
            LogDebug("[GET AUTH CODE BEGIN]");
            LogDebug($"Listens for callbacks on {Config.RedirectUri} ...");
            
            // make the call for auth code and await callback from redirect ...
            var authState = new AuthState(Config.IsStateUsed, Config.IsPkceUsed, Config.ClientId);
            var authorizationUri = buildAuthRequest(authState);
            
            LogDebug(authorizationUri);

            var loopbackHostUri = new Uri(Config.RedirectUri!.AbsoluteUri); 
            var target = new Uri(authorizationUri);
            var outcome = await Config.Browser.GetLoopbackAsync(target, loopbackHostUri, loopbackFilter, CancellationToken.None); // todo support timeout
            if (!outcome)
                return Outcome<Grant>.Fail(new AuthenticationException("Authority never returned authorization code"));

            var callback = outcome.Value!;
            if (!callback.Query.TryGetValue("code", out var authCode))
                return Outcome<Grant>.Fail(new AuthenticationException("No authorization code found in authority callback"));
            
            if (!callback.Query.TryGetValue("state", out var inState))
                return Outcome<Grant>.Fail(new AuthenticationException("No state found in authority callback"));
            
            // check the PKCE and get the access code ...
            // var authCode = callbackOutcome.Value!.TryGetQueryValue("code").Value; // todo Possible null value here
            // var inState = callbackOutcome.Value!.TryGetQueryValue("state").Value;
            LogDebug("[GET AUTH CODE END]");
            if (authState.IsUsed && inState != authState.State)
                return Outcome<Grant>.Fail(
                    new WebException($"Returned state was invalid: \"{inState}\". Expected state: \"{authState.State}\""));
            
            LogDebug("[GET ACCESS CODE BEGIN]");
            var accessCodeResult = await getAccessCode(authCode, authState);
            LogDebug("[GET ACCESS CODE END]");
            return onAuthorizationDone(accessCodeResult);

            Task<LoopbackFilterOutcome> loopbackFilter(HttpRequest request)
            {
                if (request.Method != HttpMethods.Get || !request.QueryString.HasValue)
                    return Task.FromResult(LoopbackFilterOutcome.RejectAndFail);

                return Task.FromResult(LoopbackFilterOutcome.Accept);
            }
        }

        async Task<Outcome<Grant>> getAccessCode(string authCode, AuthState authState)
        {
            var body = buildTokenRequestBody(authCode, authState);
            var uri = Config.TokenIssuer.AbsoluteUri;
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            var bodyData = Encoding.ASCII.GetBytes(body);
            request.ContentLength = bodyData.Length;
            
            Log.DebugWebRequest(request, body);

            var stream = request.GetRequestStream();
            await stream.WriteAsync(bodyData, 0, bodyData.Length);
            stream.Close();
            Log?.DebugWebRequest(request, body);
            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream() 
                             ?? throw new Exception("Unexpected error: No response when requesting token.");
                
                using (var r = new StreamReader(responseStream))
                {
                    var text = await r.ReadToEndAsync();
                    Log?.DebugWebResponse(response as HttpWebResponse, text);
                    return await buildAuthResultAsync(text);
                }
            }
            catch (WebException webException)
            {
                var response = (HttpWebResponse)webException.Response;
                var serverError = new HttpServerException((HttpStatusCode) (int) response.StatusCode, HttpServerException.DefaultMessage((HttpStatusCode)response.StatusCode));
                return Outcome<Grant>.Fail(serverError);
            }
            catch (Exception ex)
            {
                return Outcome<Grant>.Fail(HttpServerException.InternalServerError($"Unexpected Server error: {ex}"));
            }
        }

        async Task<Outcome<Grant>> acquireRenewedAccessTokenAsync(string refreshToken)
        {
            var body = makeRefreshTokenBody(refreshToken, Config.IsPkceUsed);
            var uri = Config.TokenIssuer;
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            var bodyData = Encoding.ASCII.GetBytes(body);
            request.ContentLength = bodyData.Length;
            var stream = request.GetRequestStream();
            await stream.WriteAsync(bodyData, 0, bodyData.Length);
            stream.Close();

            Log?.DebugWebRequest(request, body);
            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream() 
                                     ?? throw new Exception("Unexpected error: No response when requesting token.");
                
                using (var r = new StreamReader(responseStream))
                {
                    var text = await r.ReadToEndAsync();
                    Log.DebugWebResponse(response as HttpWebResponse, text);
                    return await buildAuthResultAsync(text);
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Failed request");
                return Outcome<Grant>.Fail(new Exception("Could not get a valid access token.", ex));
            }
        }

        string makeRefreshTokenBody(string refreshToken, bool includeClientId)
        {
            var sb = new StringBuilder();
            sb.Append("grant_type=refresh_token");
            sb.Append($"&refresh_token={refreshToken}");
            if (includeClientId)
            {
                sb.Append($"&client_id={Config.ClientId}");
            }
            return sb.ToString();
        }

        static async Task<Outcome<ActorToken>> validateIdTokenAsync(ActorToken idToken)
        {
            var validator = new IdTokenValidator();
            var validateOutcome = await validator.ValidateAsync(idToken);
            return validateOutcome 
                ? Outcome<ActorToken>.Success(idToken) 
                : Outcome<ActorToken>.Fail(validateOutcome.Exception!);
        }

        async Task<Outcome<Grant>> buildAuthResultAsync(string responseText)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(responseText);
            if (!dict.TryGetValue("access_token", out var accessToken))
                return Outcome<Grant>.Fail(new Exception("Could not get a valid access token."));

            var tokens = new List<TokenInfo>();
            var expires = dict.TryGetValue("expires_in", out var exp) && int.TryParse(exp, out var seconds)
                ? DateTime.Now.AddSeconds(seconds - 4)
                : (DateTime?)null;
            
            tokens.Add(new TokenInfo(accessToken, TokenRole.AccessToken, expires));

            if (dict.TryGetValue("refresh_token", out var refreshToken))
            {
                tokens.Add(new TokenInfo(refreshToken, TokenRole.RefreshToken));
            }

            if (!dict.TryGetValue("id_token", out var idToken)) 
                return await cacheAuthResultAsync(Outcome<Grant>.Success(new Grant(/*Config, obsolete */tokens.ToArray())));
            
            tokens.Add(new TokenInfo(idToken, TokenRole.IdToken, null, validateIdTokenAsync));
            return await cacheAuthResultAsync(Outcome<Grant>.Success(new Grant(/*Config, obsolete*/ tokens.ToArray())));
        }
        
        async Task<Outcome<Grant>> tryGetCachedAuthResultAsync()
        {
            if (!IsCaching)
                return Outcome<Grant>.Fail(new Exception("Caching is turned off"));

            return await Config.GetCachedTokenAsync(CacheKey);
        }

        async Task<Outcome<Grant>> cacheAuthResultAsync(Outcome<Grant> authResult)
        {
            if (!IsCaching)
                return authResult;

            await Config.TokenCache.AttemptCreateOrUpdateAsync(authResult.Value!, CacheKey);
            return authResult;
        }

        async Task removeFromCacheAsync()
        {
            await Config.TokenCache.AttemptDeleteAsync(CacheKey);
        }

        string buildTokenRequestBody(string authCode, AuthState authState)
        {
            var sb = new StringBuilder();
            sb.Append("grant_type=authorization_code");
            sb.Append($"&code={authCode}");
            sb.Append($"&client_id={Config.ClientId}");
            sb.Append($"&redirect_uri={Uri.EscapeDataString(Config.RedirectUri!.AbsoluteUri)}");
            if (authState.Verifier is not null)
                sb.Append($"&code_verifier={authState.Verifier}");

            return sb.ToString();
        }

        string buildAuthRequest(AuthState authState)
        {
            var sb = new StringBuilder();
            sb.Append($"{Config.Authority!.AbsoluteUri}?response_type=code");
            sb.Append($"&redirect_uri={Uri.EscapeDataString(Config.RedirectUri!.AbsoluteUri)}");
            sb.Append($"&client_id={Config.ClientId.Trim()}");

            if (Config.IsRequestingUserId)
                Config.AddScope(GrantScope.OpenId);
                
            if (!string.IsNullOrEmpty(Config.Scope))
                sb.Append($"&scope={Config.Scope.UrlEncoded()}");

            // state ...
            if (!authState.IsUsed)
                return sb.ToString();

            sb.Append($"&state={HttpUtility.UrlEncode(authState.State)}");
            if (!authState.IsPkce)
                return sb.ToString();

            sb.Append($"&code_challenge={authState.CodeChallenge}");
            sb.Append($"&code_challenge_method={authState.CodeChallengeMethod}");
            return sb.ToString();
        }

        /// <summary>
        ///   Initializes the authenticator.
        /// </summary>
        /// <param name="config">
        ///   Contains the configuration.
        /// </param>
        public TetraPakAuthenticator(AuthConfig config) 
        : base(config)
        {
        }
    }

    /// <summary>
    ///   Arguments for the <see cref="TetraPakAuthenticator.Authorized"/> event.
    /// </summary>
    public class AuthResultEventArgs : EventArgs
    {
        /// <summary>
        ///   Gets the authorization result.
        /// </summary>
        public Outcome<Grant> Result { get; }

        internal AuthResultEventArgs(Outcome<Grant> result)
        {
            Result = result;
        }
    }
}
