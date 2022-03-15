﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.Logging;
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Http.Debugging;
using TetraPak.XP.Web.Services;

namespace TetraPak.XP.OAuth2.Refresh
{
    class TetraPakRefreshTokenGrantService : GrantServiceBase, IRefreshTokenGrantService
    {
        /// <inheritdoc />
        async Task<Outcome<Grant>> IRefreshTokenGrantService.AcquireTokenAsync(
            ActorToken refreshToken,
            GrantOptions options)
        {
            Log.Debug("---- START - Tetra Pak Refresh Token Flow ----");
            var authContextOutcome = TetraPakConfig.GetAuthContext(GrantType.AC, options);
            if (!authContextOutcome)
                return Outcome<Grant>.Fail(authContextOutcome.Exception!);

            var ctx = authContextOutcome.Value!;
            var conf = ctx.Configuration;
            var messageId = GetMessageId();
                        
            var appCredentialsOutcome = await GetAppCredentialsAsync(ctx);
            if (!appCredentialsOutcome)
                return Outcome<Grant>.Fail(appCredentialsOutcome.Exception!);
            
            var appCredentials = appCredentialsOutcome.Value!;
            var clientId = appCredentials.Identity; 

            var tokenIssuerUriString = conf.TokenIssuerUri;
            if (string.IsNullOrWhiteSpace(tokenIssuerUriString))
                return conf.MissingConfigurationOutcome<Grant>(nameof(AuthContext.Configuration.TokenIssuerUri));

            if (!Uri.TryCreate(tokenIssuerUriString, UriKind.Absolute, out var tokenIssuerUri))
                return conf.InvalidConfigurationOutcome<Grant>(nameof(AuthContext.Configuration.TokenIssuerUri), tokenIssuerUriString);

            var bodyOutcome = makeRefreshTokenBody(refreshToken, clientId, ctx);
            if (!bodyOutcome)
                return Outcome<Grant>.Fail(bodyOutcome.Exception!);
            
            var clientOutcome = await GetHttpClientAsync();
            if (!clientOutcome)
                return Outcome<Grant>.Fail(
                    new HttpServerConfigurationException(
                        "Refresh token grant service failed to obtain a HTTP client (see inner exception)", 
                        clientOutcome.Exception));

            using var client = clientOutcome.Value!;
            var request = new HttpRequestMessage(HttpMethod.Post, tokenIssuerUri)
            {
                Content = bodyOutcome.Value!
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
                var response = await client.SendAsync(request, ctx.CancellationToken);
                if (sb is { })
                {
                    sb.AppendLine();
                    await (await response.ToGenericHttpResponseAsync()).ToStringBuilderAsync(sb);
                    Log.Trace(sb.ToString(), messageId);
                }
                if (!response.IsSuccessStatusCode)
                    return logFailedOutcome(response);

                var outcome = await buildGrantAsync(response);
                return await onAuthorizationDoneAsync(outcome, ctx);
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
            
        async Task<Outcome<Grant>> onAuthorizationDoneAsync(Outcome<Grant> outcome, AuthContext context)
        {
            var grant = outcome.Value!;
            await CacheGrantAsync(context, grant);
            if (grant.RefreshToken is null) 
                return outcome;
                
            await CacheRefreshTokenAsync(context, grant.RefreshToken);
            return outcome;
        }
        
        static Outcome<FormUrlEncodedContent> makeRefreshTokenBody(
            IStringValue refreshToken, 
            string? clientId, 
            AuthContext authContext)
        {
            var dict = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken.StringValue
            };
            
            if (clientId is null)
                return Outcome<FormUrlEncodedContent>.Success(new FormUrlEncodedContent(dict!));

            var conf = authContext.Configuration;
            if (string.IsNullOrWhiteSpace(clientId))
                return conf.MissingConfigurationOutcome<FormUrlEncodedContent>(nameof(AuthContext.Configuration.ClientId));

            dict["client_id"] = clientId;
            return Outcome<FormUrlEncodedContent>.Success(new FormUrlEncodedContent(dict!));
        }
        
        public TetraPakRefreshTokenGrantService(
            ITetraPakConfiguration tetraPakConfig, 
            IHttpClientProvider httpClientProvider,
            ITokenCache? tokenCache = null,
            IAppCredentialsDelegate? appCredentialsDelegate = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(tetraPakConfig, httpClientProvider, null, tokenCache, appCredentialsDelegate, log, httpContextAccessor)
        {
        }
    }
}