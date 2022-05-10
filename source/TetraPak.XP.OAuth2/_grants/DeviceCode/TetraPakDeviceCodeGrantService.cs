using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Diagnostics;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.OAuth2.Refresh;
using TetraPak.XP.Web.Abstractions;
using TetraPak.XP.Web.Abstractions.Debugging;
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Services;

namespace TetraPak.XP.OAuth2.DeviceCode
{
    /// <summary>
    ///   Implements the OAuth "Device Code" grant type targeting a Tetra Pak authority. 
    /// </summary>
    public sealed class TetraPakDeviceCodeGrantService : GrantServiceBase, IDeviceCodeGrantService
    {
        protected override GrantType GetGrantType() => GrantType.DeviceCode;
        
        /// <inheritdoc />
        public async Task<Outcome<Grant>> AcquireTokenAsync(
            GrantOptions options,
            Func<VerificationArgs,Task> verificationUriAsyncHandler)
        {
            // todo Consider decomposing this method (it's too big) 
            var messageId = GetMessageId();
            var authContextOutcome = TetraPakConfig.GetAuthContext(GrantType.DeviceCode, options);
            if (!authContextOutcome)
                return Outcome<Grant>.Fail(authContextOutcome.Exception!);

            var authContext = authContextOutcome.Value!;
            var clientCredentialsOutcome = await GetClientCredentialsAsync(authContext);
            if (!clientCredentialsOutcome)
                return Outcome<Grant>.Fail(clientCredentialsOutcome.Exception!);

            SetCancellation(authContext.Options.CancellationTokenSource);
            var clientCredentials = clientCredentialsOutcome.Value!;
            try
            {
                var cachedOutcome = await GetCachedGrantAsync(authContext);
                if (cachedOutcome)
                {
                    var cachedGrant = cachedOutcome.Value!;
                    if (!cachedGrant.IsExpired)
                        return cachedOutcome;
                }

                var cachedRefreshTokenOutcome = await GetCachedRefreshTokenAsync(authContext);
                if (cachedRefreshTokenOutcome)
                {
                    var refreshToken = cachedRefreshTokenOutcome.Value!;
                    var refreshOutcome = await RefreshTokenGrantService!.AcquireTokenAsync(refreshToken, options);
                    if (refreshOutcome)
                        return await onAuthorizationDoneAsync(refreshOutcome, authContext);
                }

                var clientOutcome = await GetHttpClientAsync();
                if (!clientOutcome)
                    return Outcome<Grant>.Fail(
                        new HttpServerConfigurationException(
                            "Device code grant service failed to obtain a HTTP client (see inner exception)",
                            clientOutcome.Exception));

                using var client = clientOutcome.Value!;
                var formsValues = new Dictionary<string, string>
                {
                    ["client_id"] = clientCredentials.Identity
                };
                if (options.Scope is { })
                {
                    formsValues.Add("scope", options.Scope.Items.ConcatCollection(" "));
                }

                var keyValues = formsValues.Select(kvp
                    => new KeyValuePair<string?, string?>(kvp.Key, kvp.Value));

                var deviceCodeIssuerUri = await authContext.GetDeviceCodeIssuerUri(); 
                if (string.IsNullOrWhiteSpace(deviceCodeIssuerUri))
                    return authContext.Configuration.MissingConfigurationOutcome<Grant>(nameof(IAuthInfo.DeviceCodeIssuerUri));
                
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

                HttpResponseMessage response;
                try
                {
                    if (IsCancellationRequested)
                        return Outcome<Grant>.Cancel("Device Code grant request was cancelled");
                        
                    response = await client.SendAsync(request, CancellationToken);
                    if (IsCancellationRequested)
                        return Outcome<Grant>.Cancel("Device Code grant request was cancelled");
                }
                catch
                {
                    if (sb is { })
                    {
                        sb.AppendLine();
                        if (IsCancellationRequested)
                        {
                            sb.AppendLine("<<< OPERATION WAS CANCELED >>>");
                        }
                        Log.Trace(sb.ToString());
                    }
                    if (IsCancellationRequested)
                        return Outcome<Grant>.Cancel("Device Code grant request was cancelled");
                    
                    throw;
                }
                if (sb is { })
                {
                    sb.AppendLine();
                    if (IsCancellationRequested)
                    {
                        sb.AppendLine("<<< OPERATION WAS CANCELED >>>");
                    }

                    await (await response.ToGenericHttpResponseAsync()).ToStringBuilderAsync(sb);
                    Log.Trace(sb.ToString(), messageId);
                }

                if (!response.IsSuccessStatusCode)
                    return await loggedFailedOutcomeAsync(response, false, CancellationToken);

#if NET5_0_OR_GREATER
                var stream = await response.Content.ReadAsStreamAsync(CancellationToken);
#else
                var stream = await response.Content.ReadAsStreamAsync();
#endif
                var codeResponseBody =
                    (await JsonSerializer.DeserializeAsync<DeviceCodeAuthCodeResponseBody>(
                        stream,
                        cancellationToken: CancellationToken))!;

                var args = new VerificationArgs(this, codeResponseBody);
#pragma warning disable CS4014
                Task.Run(() => verificationUriAsyncHandler(args), CancellationToken);
#pragma warning restore CS4014
                var timeout = XpDateTime.Now.Add(args.ExpiresIn);
                var interval = TimeSpan.FromSeconds(codeResponseBody.Interval);
                while (XpDateTime.Now < timeout && !IsCancellationRequested)
                {
                    await Task.Delay(interval, CancellationToken);
                    if (IsCancellationRequested)
                        return Outcome<Grant>.Fail(new Exception());

                    var codeVerificationOutcome = await pollCodeVerificationAsync(codeResponseBody);
                    if (codeVerificationOutcome.Value?.IsPendingVerification() ?? false)
                        continue;

                    return await onAuthorizationDoneAsync(codeVerificationOutcome, authContext);
                }

                return IsCancellationRequested
                    ? Outcome<Grant>.Cancel("Device Code Grant request was cancelled")
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

                var tokenRequestBodyOutcome = await makeTokenRequestBodyAsync(clientCredentials.Identity, deviceCode, authContext);
                if (!tokenRequestBodyOutcome)
                    return Outcome<Grant>.Fail(tokenRequestBodyOutcome.Exception!);

                var tokenIssuerUri = await authContext.GetTokenIssuerUriAsync();
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
                    var response = await client.SendAsync(request, CancellationToken);
                    if (sb is { })
                    {
                        sb.AppendLine();
                        await (await response.ToGenericHttpResponseAsync(contentAsString: true))
                            .ToStringBuilderAsync(sb);
                        Log.Trace(sb.ToString(), messageId);
                    }

                    if (!response.IsSuccessStatusCode)
                        return await loggedFailedOutcomeAsync(
                            response,
                            await isPendingUserCodeAsync(response, CancellationToken), CancellationToken);

#if NET5_0_OR_GREATER
                    await using var stream = await response.Content.ReadAsStreamAsync(CancellationToken);
#else
                    using var stream = await response.Content.ReadAsStreamAsync();
#endif
                    var body = (await JsonSerializer.DeserializeAsync<DeviceCodePollVerificationResponseBody>(
                        stream,
                        cancellationToken: CancellationToken))!;

                    var grant = body.ToGrant();
                    if (grant)
                        return Outcome<Grant>.Success(grant.Value!);

                    Log.Error(grant.Exception!);
                    return Outcome<Grant>.Fail(grant.Exception!);
                }
                catch (TaskCanceledException)
                {
                    if (sb is { })
                    {
                        sb.AppendLine();
                        if (IsCancellationRequested)
                        {
                            sb.AppendLine("<<< OPERATION WAS CANCELED >>>");
                        }
                        Log.Trace(sb.ToString(), messageId);
                    }
                    Log.Information("Device Code grant was cancelled");
                    return Outcome<Grant>.Cancel();
                }
                catch (Exception ex)
                {
                    ex = new Exception("Device Code grant failure when polling for token (see inner)", ex);
                    Log.Error(ex);
                    return Outcome<Grant>.Fail(ex);
                }
            }
            
            async Task<Outcome<Grant>> loggedFailedOutcomeAsync(
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
                    await dump.AddAsync(TetraPakConfig, "AuthConfig");
                    await dump.AddAsync(clientCredentials, "Credentials");
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
        
        static async Task<Outcome<FormUrlEncodedContent>> makeTokenRequestBodyAsync(
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
        
            var tokenIssuerUri = await ctx.GetTokenIssuerUriAsync();
            return string.IsNullOrWhiteSpace(tokenIssuerUri) 
                ? ctx.Configuration.MissingConfigurationOutcome<FormUrlEncodedContent>(nameof(IAuthConfiguration.TokenIssuerUri))
                : Outcome<FormUrlEncodedContent>.Success(new FormUrlEncodedContent(formsValues));
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

        /// <summary>
        ///   Initializes the grant service.
        /// </summary>
        /// <param name="httpClientProvider">
        ///   A HttpClient factory.
        /// </param>
        /// <param name="refreshTokenGrantService">
        ///   Enables the OAuth Refresh Grant flow. 
        /// </param>
        /// <param name="tetraPakConfig">
        ///   (optional)<br/>
        ///   A Tetra Pak integration configuration.
        /// </param>
        /// <param name="tokenCache">
        ///   (optional)<br/>
        ///   A specialized (secure) token cache to reduce traffic and improve performance
        /// </param>
        /// <param name="appCredentialsDelegate">
        ///   (optional)<br/>
        ///   A delegate to handle custom logic for obtaining application credentials (client id / client secret).   
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logger provider.   
        /// </param>
        /// <param name="httpContextAccessor">
        ///   Provides access to the current request/response <see cref="HttpContext"/>. 
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Any parameter was <c>null</c>.
        /// </exception>
        public TetraPakDeviceCodeGrantService(
            IHttpClientProvider httpClientProvider,
            ITetraPakConfiguration? tetraPakConfig = null, 
            IRefreshTokenGrantService? refreshTokenGrantService = null,
            ITokenCache? tokenCache = null,
            IAppCredentialsDelegate? appCredentialsDelegate = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(httpClientProvider, tetraPakConfig, refreshTokenGrantService, tokenCache, appCredentialsDelegate, log, httpContextAccessor)
        {
        }
    }
}