using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.Auth.Refresh;
using TetraPak.XP.Caching;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;
using TetraPak.XP.Web;
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Http.Debugging;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth.AuthCode
{
    class TetraPakAuthorizationCodeGrantService : GrantServiceBase, IAuthorizationCodeGrantService
    {
        readonly ILoopbackBrowser _browser;
        const string CacheRepository = CacheRepositories.Tokens.OIDC;
        
        /// <inheritdoc />
        public async Task<Outcome<Grant>> AcquireTokenAsync(GrantOptions options)
        {
            var messageId = GetMessageId();
            var appCredentialsOutcome = await GetAppCredentialsAsync();
            if (!appCredentialsOutcome)
                return Outcome<Grant>.Fail(appCredentialsOutcome.Exception!);
            var appCredentials = appCredentialsOutcome.Value!;

            var authContextOutcome = await TetraPakConfig.GetAuthContextAsync(GrantType.DeviceCode, options);
            if (!authContextOutcome)
                return Outcome<Grant>.Fail(authContextOutcome.Exception!);
            var authContext = authContextOutcome.Value!;

            var cachedOutcome = !string.IsNullOrWhiteSpace(options.ActorId) && IsCachingGrants(options)
                ? await GetCachedGrantAsync(CacheRepository, options.ActorId)
                : Outcome<Grant>.Fail("Cached grant not allowed/available");

            if (cachedOutcome)
                return cachedOutcome;

            await removeFromCacheAsync(appCredentials);
            if (!IsRefreshingGrants(cachedOutcome.Value!.RefreshToken, options))
                return await onAuthorizationDone(
                    await acquireTokenViaWebUIAsync(appCredentials, authContext, messageId));

            // attempt refresh token ...
            var refreshToken = cachedOutcome.Value!.RefreshToken;
            var refreshOutcome = await RefreshTokenGrantService?.AcquireTokenAsync(refreshToken!, options)!;
            if (refreshOutcome)
                return await onAuthorizationDone(refreshOutcome);

            // run the OIDC 'dance' through a browser ... 
            return await onAuthorizationDone(await acquireTokenViaWebUIAsync(appCredentials, authContext, messageId));
            
            async Task<Outcome<Grant>> onAuthorizationDone(Outcome<Grant> outcome)
            {
                if (outcome && isCachingTokens(options))
                {
                    await CacheGrantAsync(CacheRepository, options.ActorId!, outcome.Value!);
                    // Authorized?.Invoke(this, new AuthResultEventArgs(outcome));
                }
                return outcome;
            }
        }
        
        Task removeFromCacheAsync(Credentials credentials) 
            => TokenCache?.AttemptDeleteAsync(credentials.Identity, CacheRepository) 
               ?? Task.CompletedTask;

        bool isCachingTokens(GrantOptions options) =>
            !string.IsNullOrWhiteSpace(options.ActorId) && IsCachingGrants(options);
        
        async Task<Outcome<Grant>> acquireTokenViaWebUIAsync(
            Credentials appCredentials, 
            AuthContext authContext, 
            LogMessageId? messageId)
        {
            Log.Debug("[BEGIN - Authorization Code request]", messageId);
            var redirectUriOutcome = await TetraPakConfig.GetRedirectUriAsync(authContext);
            if (!redirectUriOutcome)
                return Outcome<Grant>.Fail(new ConfigurationException("Redirect URI was not configured"));
            var redirectUri = redirectUriOutcome.Value!;
            
            Log.Debug($"Listens for authorization code on {redirectUri} ...");
            
            // make the call for auth code and await callback from redirect ...
            var authorityUriOutcome = await TetraPakConfig.GetTokenIssuerUrlAsync(authContext);
            if (!authorityUriOutcome)
                return Outcome<Grant>.Fail(authorityUriOutcome.Exception!);
            var authorityUri = authorityUriOutcome.Value!;
            
            var tokenIssuerUriOutcome = await TetraPakConfig.GetTokenIssuerUrlAsync(authContext);
            if (!tokenIssuerUriOutcome)
                return Outcome<Grant>.Fail(tokenIssuerUriOutcome.Exception!);
            var tokenIssuerUri = tokenIssuerUriOutcome.Value!;

            var isStateUsedOutcome = await TetraPakConfig.IsStateUsedAsync(authContext, false);
            var isPkceUsedOutcome = await TetraPakConfig.IsPkceUsedAsync(authContext, false);
            var authState = new AuthState(isStateUsedOutcome.Value, isPkceUsedOutcome.Value, appCredentials.Identity);
            var authCodeRequestOutcome = await buildAuthRequestAsync(authorityUri, authState, authContext);
            if (!authCodeRequestOutcome)
                return Outcome<Grant>.Fail(authCodeRequestOutcome.Exception!);
            
            var loopbackHostUri = new Uri(redirectUri.AbsoluteUri); 
            var target = new Uri(authCodeRequestOutcome.Value!);
            var outcome = await _browser.GetLoopbackAsync(target, loopbackHostUri, loopbackFilter, CancellationToken.None); // todo support timeout
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
            Log.Debug("[END - Authorization Code request]", messageId);
            if (authState.IsUsed && inState != authState.State)
                return Outcome<Grant>.Fail(
                    new WebException($"Returned state was invalid: \"{inState}\". Expected state: \"{authState.State}\""));
            
            var accessCodeOutcome = await getAccessCodeAsync(tokenIssuerUri, authCode, authState, authContext, messageId);
            Log.Debug("[GET ACCESS CODE END]");
            return accessCodeOutcome;

            Task<LoopbackFilterOutcome> loopbackFilter(HttpRequest request)
            {
                if (request.Method != HttpMethods.Get || !request.QueryString.HasValue)
                    return Task.FromResult(LoopbackFilterOutcome.RejectAndFail);

                return Task.FromResult(LoopbackFilterOutcome.Accept);
            }
        }

        async Task<Outcome<string>> buildAuthRequestAsync(Uri authorityUri, AuthState authState, AuthContext authContext)
        {
            var sb = new StringBuilder();
            var redirectUriOutcome = await TetraPakConfig.GetRedirectUriAsync(authContext);
            if (!redirectUriOutcome)
                return Outcome<string>.Fail(redirectUriOutcome.Exception!);
            var redirectUri = redirectUriOutcome.Value!;
            
            var clientIdOutcome = await TetraPakConfig.GetClientIdAsync(authContext);
            if (!clientIdOutcome)
                return Outcome<string>.Fail(clientIdOutcome.Exception!);
            var clientId = clientIdOutcome.Value!;
            
            var scopeOutcome = await TetraPakConfig.GetScopeAsync(authContext);
            var scopes = scopeOutcome ? scopeOutcome.Value!.Items.ToList() : new List<string>();
            if (scopes.Count == 0)
            {
                scopes.Add(GrantScope.OpenId); // todo consider whether "openid" grant request should be optional
            }
            var scope = new GrantScope(scopes.ToArray());
            
            sb.Append($"{authorityUri.AbsoluteUri}?response_type=code");
            sb.Append($"&redirect_uri={Uri.EscapeDataString(redirectUri.AbsoluteUri)}");
            sb.Append($"&client_id={clientId}");
            sb.Append($"&scope={scope}");
                
            // state ...
            if (!authState.IsUsed)
                return Outcome<string>.Success(sb.ToString());

            sb.Append($"&state={HttpUtility.UrlEncode(authState.State)}");
            if (!authState.IsPKCEUsed)
                return Outcome<string>.Success(sb.ToString());

            sb.Append($"&code_challenge={authState.CodeChallenge}");
            sb.Append($"&code_challenge_method={authState.CodeChallengeMethod}");
            return Outcome<string>.Success(sb.ToString());
        }
        
         async Task<Outcome<Grant>> getAccessCodeAsync(
             Uri tokenIssuerUri, 
             string authCode, 
             AuthState authState,
             AuthContext authContext,
             LogMessageId? messageId)
        {
            var clientOutcome = await GetHttpClientAsync();
            if (!clientOutcome)
                return Outcome<Grant>.Fail(
                    new HttpServerConfigurationException(
                        "Authorization code service failed to obtain a HTTP client (see inner exception)", 
                        clientOutcome.Exception));
                
            using var client = clientOutcome.Value!;
            var bodyOutcome = await buildTokenRequestBodyAsync(authCode, authState, authContext);
            if (!bodyOutcome)
                return Outcome<Grant>.Fail(bodyOutcome.Exception!);

            var request = new HttpRequestMessage(HttpMethod.Post, tokenIssuerUri)
            {
                Content = new FormUrlEncodedContent(bodyOutcome.Value!)
            };
            var sb = Log?.IsEnabled(LogRank.Trace) ?? false
                ? await (await request.ToGenericHttpRequestAsync()).ToStringBuilderAsync(
                    new StringBuilder(), 
                    () => TraceHttpRequestOptions.Default(messageId)
                        .WithInitiator(this, HttpDirection.Out)
                        .WithDefaultHeaders(client.DefaultRequestHeaders))
                : null;

            try
            {
                var response = await client.SendAsync(request, authContext.CancellationToken);
                if (sb is { })
                {
                    sb.AppendLine();
                    await (await response.ToGenericHttpResponseAsync()).ToStringBuilderAsync(sb);
                    Log.Trace(sb.ToString(), messageId);
                }
                if (!response.IsSuccessStatusCode)
                    return logFailedOutcome(response);

                return await buildGrantAsync(response);
            }
            catch (Exception ex)
            {
                return Outcome<Grant>.Fail(HttpServerException.InternalServerError($"Unexpected Server error: {ex}"));
            }
            
            async Task<Outcome<Grant>> buildGrantAsync(HttpResponseMessage response)
            {
#if NET5_0_OR_GREATER
                await using var stream = await response.Content.ReadAsStreamAsync();
#else
                using var stream = await response.Content.ReadAsStreamAsync();
#endif
                var dict = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream);
                if (!dict!.TryGetValue("access_token", out var accessToken) || string.IsNullOrWhiteSpace(accessToken))
                    return Outcome<Grant>.Fail(new Exception("Could not get a valid access token."));

                var expires = dict.TryGetValue("expires_in", out var exp) && int.TryParse(exp, out var seconds)
                    ? DateTime.Now.AddSeconds(seconds)
                    : (DateTime?)null;
                var tokens = new List<TokenInfo>();
                tokens.Add(new TokenInfo(accessToken!, TokenRole.AccessToken, expires));

                if (dict.TryGetValue("refresh_token", out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
                {
                    tokens.Add(new TokenInfo(refreshToken!, TokenRole.RefreshToken));
                }

                if (!dict.TryGetValue("id_token", out var idToken) || string.IsNullOrWhiteSpace(idToken)) 
                    return Outcome<Grant>.Success(new Grant(tokens.ToArray()));
            
                tokens.Add(new TokenInfo(idToken!, TokenRole.IdToken, null, validateIdTokenAsync));
                return Outcome<Grant>.Success(new Grant(tokens.ToArray()));
            }
            
            static async Task<Outcome<ActorToken>> validateIdTokenAsync(ActorToken idToken)
            {
                var validator = new IdTokenValidator();
                var validateOutcome = await validator.ValidateAsync(idToken);
                return validateOutcome 
                    ? Outcome<ActorToken>.Success(idToken) 
                    : Outcome<ActorToken>.Fail(validateOutcome.Exception!);
            }
            
            Outcome<Grant> logFailedOutcome(HttpResponseMessage response)
            {
                var ex = new HttpServerException(response); 
                if (Log?.IsEnabled(LogRank.Debug) ?? false)
                    return Outcome<Grant>.Fail(ex);

                // var messageId = _tetraPakConfig.AmbientData.GetMessageId(true);
                var message = new StringBuilder();
                message.AppendLine("Authorization Code service failure (state dump to follow if DEBUG log level is enabled)");
                var dump = new StateDump().WithStackTrace();
                dump.AddAsync(TetraPakConfig, "AuthConfig");
                message.AppendLine(dump.ToString());
                Log.Error(ex, message.ToString(), messageId);
                return Outcome<Grant>.Fail(ex);
            }

        }
         
         async Task<Outcome<Dictionary<string,string>>> buildTokenRequestBodyAsync(
             string authCode, 
             AuthState authState, 
             AuthContext authContext)
         {
             var clientIdOutcome = await TetraPakConfig.GetClientIdAsync(authContext);
             if (!clientIdOutcome)
                 return Outcome<Dictionary<string,string>>.Fail(clientIdOutcome.Exception!);
             var clientId = clientIdOutcome.Value!;

             var redirectUriOutcome = await TetraPakConfig.GetRedirectUriAsync(authContext);
             if (!redirectUriOutcome)
                 return Outcome<Dictionary<string,string>>.Fail(redirectUriOutcome.Exception!);

             var redirectUri = redirectUriOutcome.Value!;
             var dictionary = new Dictionary<string, string>
             {
                 ["grant_type"] = "authorization_code",
                 ["code"] = authCode,
                 ["client_id"] = clientId,
                 ["redirect_uri"] = Uri.EscapeDataString(redirectUri.AbsoluteUri)
             };
             if (authState.Verifier is not null)
                 dictionary["code_verifier"] = authState.Verifier;
                 
             // sb.Append("grant_type=authorization_code"); obsolete
             // sb.Append($"&code={authCode}");
             // sb.Append($"&client_id={clientId}");
             // sb.Append($"&redirect_uri={Uri.EscapeDataString(redirectUri.AbsoluteUri)}");
             // if (authState.Verifier is not null)
             //     sb.Append($"&code_verifier={authState.Verifier}");

             return Outcome<Dictionary<string,string>>.Success(dictionary);
         }

        public TetraPakAuthorizationCodeGrantService(
            ITetraPakConfiguration tetraPakConfig, 
            IHttpClientProvider httpClientProvider,
            IRefreshTokenGrantService? refreshTokenGrantService,
            ILoopbackBrowser browser,
            ITokenCache? tokenCache = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(tetraPakConfig, httpClientProvider, refreshTokenGrantService, tokenCache, log, httpContextAccessor)
        {
            _browser = browser;
        }
    }
}