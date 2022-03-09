using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
using TetraPak.XP.Caching;
using TetraPak.XP.Logging;
using TetraPak.XP.OAuth2.Refresh;
using TetraPak.XP.Web;
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Http.Debugging;

namespace TetraPak.XP.OAuth2.AuthCode
{
    class TetraPakAuthorizationCodeGrantService : GrantServiceBase, IAuthorizationCodeGrantService
    {
        readonly ILoopbackBrowser _browser;
        const string GrantCacheRepo = CacheRepositories.Tokens.OIDC;
        const string RefreshTokenCacheRepo = CacheRepositories.Tokens.Refresh;
        
        /// <inheritdoc />
        public async Task<Outcome<Grant>> AcquireTokenAsync(GrantOptions options)
        {
            var messageId = GetMessageId();
            var authContextOutcome = TetraPakConfig.GetAuthContext(GrantType.AC, options);
            if (!authContextOutcome)
                return Outcome<Grant>.Fail(authContextOutcome.Exception!);
            
            var authContext = authContextOutcome.Value!;
            
            var appCredentialsOutcome = await GetAppCredentialsAsync(authContext);
            if (!appCredentialsOutcome)
                return Outcome<Grant>.Fail(appCredentialsOutcome.Exception!);
            
            var appCredentials = appCredentialsOutcome.Value!;
            
            var redirectUriString = authContext.Configuration.RedirectUri;
            var conf = authContext.Configuration;
            if (string.IsNullOrWhiteSpace(redirectUriString))
                return AuthConfiguration.MissingConfigurationOutcome<Grant>(conf, nameof(AuthContext.Configuration.RedirectUri));
            
            if (!Uri.TryCreate(redirectUriString, UriKind.Absolute, out var redirectUri))
                return AuthConfiguration.InvalidConfigurationOutcome<Grant>(conf, nameof(AuthContext.Configuration.RedirectUri), redirectUriString);

            var authorityUriString = conf.AuthorityUri;
            if (string.IsNullOrWhiteSpace(authorityUriString))
                return AuthConfiguration.MissingConfigurationOutcome<Grant>(conf, nameof(AuthContext.Configuration.AuthorityUri));
            
            if (!Uri.TryCreate(authorityUriString, UriKind.Absolute, out var authorityUri))
                return AuthConfiguration.InvalidConfigurationOutcome<Grant>(conf, nameof(AuthContext.Configuration.AuthorityUri), authorityUriString);
            
            var tokenIssuerUriString = conf.TokenIssuerUri;
            if (string.IsNullOrWhiteSpace(tokenIssuerUriString))
                return AuthConfiguration.MissingConfigurationOutcome<Grant>(conf, nameof(AuthContext.Configuration.TokenIssuerUri));
            
            if (!Uri.TryCreate(tokenIssuerUriString, UriKind.Absolute, out var tokenIssuerUri))
                return AuthConfiguration.InvalidConfigurationOutcome<Grant>(conf, nameof(AuthContext.Configuration.TokenIssuerUri), tokenIssuerUriString);

            var isStateUsed = conf.OidcState;
            var isPkceUsed = conf.OidcPkce;
            var authState = new AuthState(isStateUsed, isPkceUsed, appCredentials.Identity);
            
            var cachedGrantOutcome = !string.IsNullOrWhiteSpace(options.ActorId) && IsCachingGrants(options)
                ? await GetCachedGrantAsync(GrantCacheRepo, options.ActorId)
                : Outcome<Grant>.Fail("Cached grant not allowed/available");

            if (cachedGrantOutcome)
                return cachedGrantOutcome;
            
            await removeFromGrantCacheAsync(options.ActorId);
            if (!await IsRefreshingGrantsAsync(options))
                return await onAuthorizationDone(
                    await acquireTokenViaWebUIAsync(authorityUri,  tokenIssuerUri,authState, appCredentials, redirectUri, authContext, messageId));

            // attempt refresh token ...
            var cachedRefreshTokenOutcome =
                await GetCachedTokenAsync(RefreshTokenCacheRepo, options.ActorId!);

            if (cachedRefreshTokenOutcome)
            {
                var refreshToken = cachedRefreshTokenOutcome.Value!;
                var refreshOutcome = await RefreshTokenGrantService!.AcquireTokenAsync(refreshToken, options);
                if (refreshOutcome)
                    return await onAuthorizationDone(refreshOutcome);
            }

            // run the OIDC 'dance' through a browser ... 
            return await onAuthorizationDone
                (await acquireTokenViaWebUIAsync(authorityUri, tokenIssuerUri, authState, appCredentials,  redirectUri, authContext, messageId));
            
            async Task<Outcome<Grant>> onAuthorizationDone(Outcome<Grant> outcome)
            {
                if (outcome && isCachingTokens(options))
                {
                    var grant = outcome.Value!;
                    await CacheGrantAsync(GrantCacheRepo, options.ActorId!, grant);
                    if (grant.RefreshToken is { })
                    {
                        await CacheTokenAsync(RefreshTokenCacheRepo, options.ActorId!, grant.RefreshToken);
                    }
                }
                return outcome;
            }
        }
        
        Task removeFromGrantCacheAsync(string? key) 
            => TokenCache?.AttemptDeleteAsync(key, GrantCacheRepo) 
               ?? Task.CompletedTask;

        bool isCachingTokens(GrantOptions options) =>
            !string.IsNullOrWhiteSpace(options.ActorId) && IsCachingGrants(options);
        
        async Task<Outcome<Grant>> acquireTokenViaWebUIAsync(
            Uri authorityUri,
            Uri tokenIssuerUri,
            AuthState authState,
            Credentials appCredentials, 
            Uri redirectUri,
            AuthContext authContext, 
            LogMessageId? messageId)
        {
            Log.Debug("[BEGIN - Authorization Code request]", messageId);
            Log.Debug($"Listens for authorization code on {redirectUri} ...");
            
            // make the call for auth code and await callback from redirect ...
            var authCodeRequestOutcome = await buildAuthRequestAsync(authorityUri, redirectUri, authState, appCredentials, authContext.Configuration);
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
            
            var accessCodeOutcome = await getAccessCodeAsync(tokenIssuerUri, redirectUri, appCredentials, authCode, authState, authContext, messageId);
            Log.Debug("[GET ACCESS CODE END]");
            return accessCodeOutcome;

            Task<LoopbackFilterOutcome> loopbackFilter(HttpRequest request)
            {
                if (request.Method != HttpMethods.Get || !request.QueryString.HasValue)
                    return Task.FromResult(LoopbackFilterOutcome.RejectAndFail);

                return Task.FromResult(LoopbackFilterOutcome.Accept);
            }
        }

        static Task<Outcome<string>> buildAuthRequestAsync(
            Uri authorityUri,
            Uri redirectUri, 
            AuthState authState,
            Credentials appCredentials,
            IAuthConfiguration authConfig)
        {
            var sb = new StringBuilder();
            var clientId =  appCredentials.Identity;
            if (string.IsNullOrWhiteSpace(clientId))
                return Task.FromResult(AuthConfiguration.MissingConfigurationOutcome<string>(authConfig, nameof(IAuthConfiguration.ClientId)));

            var scope = authConfig.OidcScope;
            
            sb.Append($"{authorityUri.AbsoluteUri}?response_type=code");
            sb.Append($"&redirect_uri={Uri.EscapeDataString(redirectUri.AbsoluteUri)}");
            sb.Append($"&client_id={clientId}");
            sb.Append($"&scope={scope}");
                
            // state ...
            if (!authState.IsUsed)
                return Task.FromResult(Outcome<string>.Success(sb.ToString()));

            sb.Append($"&state={HttpUtility.UrlEncode(authState.State)}");
            if (!authState.IsPkce)
                return Task.FromResult(Outcome<string>.Success(sb.ToString()));

            sb.Append($"&code_challenge={authState.CodeChallenge}");
            sb.Append($"&code_challenge_method={authState.CodeChallengeMethod}");
            return Task.FromResult(Outcome<string>.Success(sb.ToString()));
        }
        
         async Task<Outcome<Grant>> getAccessCodeAsync(
             Uri tokenIssuerUri,
             Uri redirectUri,
             Credentials appCredentials,
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
            var bodyOutcome = await buildTokenRequestBodyAsync(authCode, appCredentials, redirectUri, authState);
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

         static Task<Outcome<Dictionary<string,string>>> buildTokenRequestBodyAsync(
             string authCode, 
             Credentials appCredentials,
             Uri redirectUri,
             AuthState authState)
         {
             var clientId = appCredentials.Identity;
             var dictionary = new Dictionary<string, string>
             {
                 ["grant_type"] = "authorization_code",
                 ["code"] = authCode,
                 ["client_id"] = clientId,
                 ["redirect_uri"] = redirectUri.AbsoluteUri
                     // ["redirect_uri"] = Uri.EscapeDataString(redirectUri.AbsoluteUri)
             };
             if (authState.Verifier is not null)
                 dictionary["code_verifier"] = authState.Verifier;
                 
             // sb.Append("grant_type=authorization_code"); obsolete
             // sb.Append($"&code={authCode}");
             // sb.Append($"&client_id={clientId}");
             // sb.Append($"&redirect_uri={Uri.EscapeDataString(redirectUri.AbsoluteUri)}");
             // if (authState.Verifier is not null)
             //     sb.Append($"&code_verifier={authState.Verifier}");

             return Task.FromResult(Outcome<Dictionary<string,string>>.Success(dictionary));
         }

        public TetraPakAuthorizationCodeGrantService(
            ITetraPakConfiguration tetraPakConfig, 
            IHttpClientProvider httpClientProvider,
            ILoopbackBrowser browser,
            IRefreshTokenGrantService? refreshTokenGrantService = null,
            ITokenCache? tokenCache = null,
            IAppCredentialsDelegate? appCredentialsDelegate = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(tetraPakConfig, httpClientProvider, refreshTokenGrantService, tokenCache, appCredentialsDelegate, log, httpContextAccessor)
        {
            _browser = browser;
        }
    }
}