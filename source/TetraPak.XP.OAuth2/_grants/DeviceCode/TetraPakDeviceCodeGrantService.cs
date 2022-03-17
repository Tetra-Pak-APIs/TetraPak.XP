using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging;
using TetraPak.XP.OAuth2.Refresh;
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Http.Debugging;
using TetraPak.XP.Web.Services;

namespace TetraPak.XP.OAuth2.DeviceCode
{
    sealed class TetraPakDeviceCodeGrantService : GrantServiceBase, IDeviceCodeGrantService
    {
        protected override GrantType GetGrantType() => GrantType.DeviceCode;
        
        public async Task<Outcome<Grant>> AcquireTokenAsync(
            GrantOptions options,
            Action<VerificationArgs> verificationUriHandler)
        {
            // todo Consider breaking up this method (it's too big) 
            // todo Honor the GrantOptions.Flags value (silent/forced request etc.)
            var messageId = GetMessageId();
            var authContextOutcome = TetraPakConfig.GetAuthContext(GrantType.DeviceCode, options);
            if (!authContextOutcome)
                return Outcome<Grant>.Fail(authContextOutcome.Exception!);

            var ctx = authContextOutcome.Value!;
            var appCredentialsOutcome = await GetAppCredentialsAsync(ctx);
            if (!appCredentialsOutcome)
                return Outcome<Grant>.Fail(appCredentialsOutcome.Exception!);

            var appCredentials = appCredentialsOutcome.Value!;
            var cts = options.CancellationTokenSource ?? new CancellationTokenSource();
            try
            {
                var cachedOutcome = await GetCachedGrantAsync(ctx);
                if (cachedOutcome)
                {
                    var cachedGrant = cachedOutcome.Value!;
                    if (!cachedGrant.IsExpired)
                        return cachedOutcome;
                }

                var cachedRefreshTokenOutcome = await GetCachedRefreshTokenAsync(ctx);
                if (cachedRefreshTokenOutcome)
                {
                    var refreshToken = cachedRefreshTokenOutcome.Value!;
                    var refreshOutcome = await RefreshTokenGrantService!.AcquireTokenAsync(refreshToken, options);
                    if (refreshOutcome)
                        return await onAuthorizationDoneAsync(refreshOutcome, ctx);
                }

                var clientOutcome = await GetHttpClientAsync();
                if (!clientOutcome)
                    return Outcome<Grant>.Fail(
                        new HttpServerConfigurationException(
                            "Device code grant service failed to obtain a HTTP client (see inner exception)",
                            clientOutcome.Exception));

                using var client = clientOutcome.Value!;
                var basicAuthCredentials = ValidateBasicAuthCredentials(appCredentials);
                client.DefaultRequestHeaders.Authorization = basicAuthCredentials.ToAuthenticationHeaderValue();
                var formsValues = new Dictionary<string, string>
                {
                    ["client_id"] = basicAuthCredentials.Identity
                };
                if (options.Scope is { })
                {
                    formsValues.Add("scope", options.Scope.Items.ConcatCollection(" "));
                }

                var keyValues = formsValues.Select(kvp
                    => new KeyValuePair<string?, string?>(kvp.Key, kvp.Value));

                var deviceCodeIssuerUri = ctx.Configuration.DeviceCodeIssuerUri;
                if (string.IsNullOrWhiteSpace(deviceCodeIssuerUri))
                    return ctx.Configuration.MissingConfigurationOutcome<Grant>(nameof(AuthContext.Configuration.DeviceCodeIssuerUri));
                
                var request = new HttpRequestMessage(HttpMethod.Post, deviceCodeIssuerUri)
                {
                    Content = new FormUrlEncodedContent(keyValues)
                };
                var sb = Log?.IsEnabled(LogRank.Trace) ?? false
                    ? await (await request.ToGenericHttpRequestAsync(contentAsString: true)).ToStringBuilderAsync(
                        new StringBuilder(),
                        () => TraceHttpRequestOptions.Default(messageId)
                            .WithInitiator(this, HttpDirection.Out)
                            .WithDefaultHeaders(client.DefaultRequestHeaders))
                    : null;

                var response = await client.SendAsync(request, cts.Token);
                if (sb is { })
                {
                    sb.AppendLine();
                    if (cts.IsCancellationRequested)
                    {
                        sb.AppendLine("<<< OPERATION WAS CANCELED >>>");
                    }

                    await (await response.ToGenericHttpResponseAsync()).ToStringBuilderAsync(sb);
                    Log.Trace(sb.ToString(), messageId);
                }

                if (!response.IsSuccessStatusCode)
                    return loggedFailedOutcome(response, false, cts.Token);

#if NET5_0_OR_GREATER
                var stream = await response.Content.ReadAsStreamAsync(cts.Token);
#else
                var stream = await response.Content.ReadAsStreamAsync();
#endif
                var codeResponseBody =
                    (await JsonSerializer.DeserializeAsync<DeviceCodeAuthCodeResponseBody>(
                        stream,
                        cancellationToken: cts.Token))!;

                var args = new VerificationArgs(codeResponseBody, cts);
#pragma warning disable CS4014
                Task.Run(() => verificationUriHandler(args), cts.Token);
#pragma warning restore CS4014
                var timeout = DateTime.Now.Add(args.ExpiresIn);
                var interval = TimeSpan.FromSeconds(codeResponseBody.Interval);
                while (DateTime.Now < timeout && !cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(interval, cts.Token);
                    if (cts.IsCancellationRequested)
                        return Outcome<Grant>.Fail(new Exception());

                    var codeVerificationOutcome = await pollCodeVerificationAsync(codeResponseBody);
                    if (codeVerificationOutcome.Value?.IsPendingVerification() ?? false)
                        continue;

                    return await onAuthorizationDoneAsync(codeVerificationOutcome, ctx);
                }

                return cts.Token.IsCancellationRequested
                    ? Outcome<Grant>.Fail(new Exception("Device Code Grant request was cancelled"))
                    : Outcome<Grant>.Fail(new Exception("Device Code Grant request timed out"));
            }
            catch (TaskCanceledException ex)
            {
                Log.Warning(ex.Message);
                return Outcome<Grant>.Fail(ex);
            }
            catch (Exception ex)
            {
                ex = new Exception($"Failed to acquire token using Device Code grant. {ex.Message}", ex);
                Log.Error(ex);
                return Outcome<Grant>.Fail(ex);
            }
            
            async Task<Outcome<Grant>> pollCodeVerificationAsync(DeviceCodeAuthCodeResponseBody codeResponse)
            {
                var deviceCode = codeResponse.DeviceCode;
                var clientOutcome = await GetHttpClientAsync();
                if (!clientOutcome)
                {
                    var exception = new Exception(
                        "Could not obtain a HTTP client when polling device code authorization (see inner). Retrying ...", 
                        clientOutcome.Exception!);
                    Log.Error(exception, messageId:messageId);
                    return Outcome<Grant>.Fail(exception);
                }

                var client = clientOutcome.Value!;

                var tokenRequestBodyOutcome = makeTokenRequestBody(appCredentials.Identity, deviceCode, ctx);
                if (!tokenRequestBodyOutcome)
                    return Outcome<Grant>.Fail(tokenRequestBodyOutcome.Exception!);

                var tokenIssuerUri = ctx.Configuration.TokenIssuerUri;
                var request = new HttpRequestMessage(HttpMethod.Post, tokenIssuerUri)
                {
                    Content = tokenRequestBodyOutcome.Value!
                };
                
                var sb = Log?.IsEnabled(LogRank.Trace) ?? false
                    ? await (await request.ToGenericHttpRequestAsync(contentAsString:true)).ToStringBuilderAsync(
                        new StringBuilder(), 
                        () => TraceHttpRequestOptions.Default(messageId)
                            .WithInitiator(this, HttpDirection.Out)
                            .WithDefaultHeaders(client.DefaultRequestHeaders))
                    : null;

                try
                {
                    var response = await client.SendAsync(request, cts.Token);
                    if (sb is { })
                    {
                        sb.AppendLine();
                        if (cts.IsCancellationRequested)
                        {
                            sb.AppendLine("<<< OPERATION WAS CANCELED >>>");
                        }
                        await (await response.ToGenericHttpResponseAsync(contentAsString:true)).ToStringBuilderAsync(sb);
                        Log.Trace(sb.ToString(), messageId);
                    }

                    if (!response.IsSuccessStatusCode)
                        return loggedFailedOutcome(response, await isPendingUserCodeAsync(response, cts.Token), cts.Token);

#if NET5_0_OR_GREATER
                    await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
#else
                    using var stream = await response.Content.ReadAsStreamAsync();
#endif
                    var body = (await JsonSerializer.DeserializeAsync<DeviceCodePollVerificationResponseBody>(
                        stream, 
                        cancellationToken: cts.Token))!;

                    var grant = body.ToGrant(); 
                    if (grant)
                        return Outcome<Grant>.Success(grant.Value!);
                
                    Log.Error(grant.Exception!);
                    return Outcome<Grant>.Fail(grant.Exception!);
                }
                catch (Exception ex)
                {
                    ex = new Exception("Device Code grant failure when polling for token (see inner)", ex);
                    Log.Error(ex);
                    return Outcome<Grant>.Fail(ex);
                }
            }
            
            Outcome<Grant> loggedFailedOutcome(
                HttpResponseMessage response,
                bool isPendingUserCodeAsync, 
                CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Outcome<Grant>.Cancel();
                    
                var outcome = Outcome<Grant>.Fail(
                    new HttpServerException(response), 
                    new Grant().ForPendingCodeVerification()); 
                if (Log is null)    
                    return outcome;

                var message = new StringBuilder();
                if (isPendingUserCodeAsync)
                {
                    if (!Log?.IsEnabled(LogRank.Trace) ?? true)
                        return outcome;
                    
                    Log.Trace("Device Code is pending user code verification ...");
                    return outcome;
                }

                message.AppendLine("Device Code grant failure (state dump to follow if DEBUG log level is enabled)");
                if (Log?.IsEnabled(LogRank.Debug) ?? false)
                {
                    var dump = new StateDump().WithStackTrace();
                    dump.AddAsync(TetraPakConfig, "AuthConfig");
                    dump.AddAsync(appCredentials, "Credentials");
                    message.AppendLine(dump.ToString());
                }
                Log.Error(outcome.Exception!, message.ToString(), messageId);
                return Outcome<Grant>.Fail(outcome.Exception!);
            }
        }
        
        async Task<Outcome<Grant>> onAuthorizationDoneAsync(Outcome<Grant> outcome, AuthContext ctx)
        {
            if (!outcome)
                return outcome;
                    
            var grant = outcome.Value!;
            await CacheGrantAsync(ctx, grant);
            if (grant.RefreshToken is { })
            {
                await CacheRefreshTokenAsync(ctx, grant.RefreshToken);
            }
            return outcome;
        }
        
        static Outcome<FormUrlEncodedContent> makeTokenRequestBody(
            string clientId,
            string deviceCode,
            AuthContext ctx)
        {
            var formsValues = new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                ["client_id"] = clientId,
                ["device_code"] = deviceCode
            };
        
            var tokenIssuerUri = ctx.Configuration.TokenIssuerUri;
            return string.IsNullOrWhiteSpace(tokenIssuerUri) 
                ? ctx.Configuration.MissingConfigurationOutcome<FormUrlEncodedContent>(nameof(IAuthConfiguration.TokenIssuerUri))
                : Outcome<FormUrlEncodedContent>.Success(new FormUrlEncodedContent(formsValues!));
        }

        static async Task<bool> isPendingUserCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.BadRequest) 
                return false;
            
#if NET5_0_OR_GREATER
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
#else                
            using var stream = await response.Content.ReadAsStreamAsync();
#endif
            try
            {
                var dict = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream, cancellationToken: cancellationToken);
                return dict!.TryGetValue("detail", out var value) &&
                       value.StartsWith("Pending", StringComparison.InvariantCultureIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
        
        public TetraPakDeviceCodeGrantService(
            ITetraPakConfiguration tetraPakConfig, 
            IHttpClientProvider httpClientProvider,
            IRefreshTokenGrantService? refreshTokenGrantService = null,
            ITokenCache? tokenCache = null,
            IAppCredentialsDelegate? appCredentialsDelegate = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(tetraPakConfig, httpClientProvider, refreshTokenGrantService, tokenCache, appCredentialsDelegate, log, httpContextAccessor)
        {
        }
    }
}