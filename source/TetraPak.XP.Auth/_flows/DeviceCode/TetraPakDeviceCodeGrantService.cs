﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging;
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Http.Debugging;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth.DeviceCode
{
    public class TetraPakDeviceCodeGrantService : GrantServiceBase, IDeviceCodeGrantService
    {
        const string CacheRepository = CacheRepositories.Tokens.DeviceCodeCredentials;

        public async Task<Outcome<DeviceCodeResponse>> AcquireTokenAsync(
            Action<VerificationArgs> verificationUriHandler,
            CancellationTokenSource? cancellationTokenSource = null, 
            Credentials? clientCredentials = null,
            MultiStringValue? scope = null, 
            bool forceAuthorization = false)
        {
            // todo Consider breaking up this method (it's too big) 
            var messageId = GetMessageId();
            var cts = cancellationTokenSource ?? new CancellationTokenSource();
            try
            {
                if (clientCredentials is null)
                {
                    var ccOutcome = await getCredentialsAsync();
                    if (!ccOutcome)
                        return Outcome<DeviceCodeResponse>.Fail(ccOutcome.Exception!);

                    clientCredentials = ccOutcome.Value!;
                }

                var cachedOutcome = forceAuthorization 
                        ? Outcome<DeviceCodeResponse>.Fail(new Exception("nisse")) // nisse Write proper error message
                        : await GetCachedResponse<DeviceCodeResponse>(CacheRepository, clientCredentials);
                if (cachedOutcome)
                {
                    var cachedResponse = cachedOutcome.Value!;
                    if (cachedResponse.ExpiresIn.Subtract(TimeSpan.FromSeconds(2)) > TimeSpan.Zero)
                        return cachedOutcome;
                }
                
                var clientOutcome = await GetHttpClientAsync();
                if (!clientOutcome)
                    return Outcome<DeviceCodeResponse>.Fail(
                        new HttpServerConfigurationException(
                            "Client credentials service failed to obtain a HTTP client (see inner exception)", 
                            clientOutcome.Exception));
                
                using var client = clientOutcome.Value!;
                var basicAuthCredentials = ValidateBasicAuthCredentials(clientCredentials);
                client.DefaultRequestHeaders.Authorization = basicAuthCredentials.ToAuthenticationHeaderValue();
                var formsValues = new Dictionary<string, string>
                {
                    ["client_id"] = basicAuthCredentials.Identity
                };
                if (scope is { })
                {
                    formsValues.Add("scope", scope.Items.ConcatCollection(" "));
                }

                var keyValues = formsValues.Select(kvp 
                    => new KeyValuePair<string?, string?>(kvp.Key, kvp.Value));

                var deviceCodeIssuerUrl = await TetraPakConfig.GetDeviceCodeIssuerUrlAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, deviceCodeIssuerUrl)
                {
                    Content = new FormUrlEncodedContent(keyValues)
                };
                var sb = Log?.IsEnabled(LogRank.Trace) ?? false
                    ? await (await request.ToGenericHttpRequestAsync(contentAsString:true)).ToStringBuilderAsync(
                        new StringBuilder(), 
                        () => TraceHttpRequestOptions.Default(messageId)
                            .WithInitiator(this, HttpDirection.Out)
                            .WithDefaultHeaders(client.DefaultRequestHeaders))
                    : null;

                var response = await client.SendAsync(request, cts.Token);
                
                if (sb is { })
                {
                    sb.AppendLine();
                    await (await response.ToGenericHttpResponseAsync()).ToStringBuilderAsync(sb);
                    Log.Trace(sb.ToString(), messageId);
                }
                
                if (!response.IsSuccessStatusCode)
                    return loggedFailedOutcome(response, false);
                
#if NET5_0_OR_GREATER
                var stream = await response.Content.ReadAsStreamAsync(cts.Token);
#else
                var stream = await response.Content.ReadAsStreamAsync();
#endif
                var codeResponseBody =
                    (await JsonSerializer.DeserializeAsync<DeviceCodeCodeResponseBody>(
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
                    var codeVerificationOutcome = await pollCodeVerificationAsync(codeResponseBody);
                    if (codeVerificationOutcome.Value?.IsPendingVerification ?? false)
                        continue;
                    
                    return codeVerificationOutcome;
                }

                return cts.Token.IsCancellationRequested
                    ? Outcome<DeviceCodeResponse>.Fail(new Exception("Device Code Grant request was cancelled"))
                    : Outcome<DeviceCodeResponse>.Fail(new Exception("Device Code Grant request timed out"));
            }
            catch (Exception ex)
            {
                ex = new Exception($"Failed to acquire token using client credentials. {ex.Message}", ex);
                Log.Error(ex);
                return Outcome<DeviceCodeResponse>.Fail(ex);
            }
            
            async Task<Outcome<DeviceCodeResponse>> pollCodeVerificationAsync(DeviceCodeCodeResponseBody codeResponse)
            {
                var deviceCode = codeResponse.DeviceCode;
                var clientOutcome = await GetHttpClientAsync();
                if (!clientOutcome)
                {
                    var exception = new Exception(
                        "Could not obtain a HTTP client when polling device code authorization (see inner). Retrying ...", 
                        clientOutcome.Exception!);
                    Log.Error(exception, messageId:messageId);
                    return Outcome<DeviceCodeResponse>.Fail(exception);
                }

                var client = clientOutcome.Value!;
            
                var formsValues = new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                    ["client_id"] = clientCredentials.Identity,
                    ["device_code"] = deviceCode
                };
        
                var keyValues = formsValues.Select(kvp 
                    => new KeyValuePair<string?, string?>(kvp.Key, kvp.Value));

                var tokenIssuerUrl = await TetraPakConfig.GetTokenIssuerUrlAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, tokenIssuerUrl)
                {
                    Content = new FormUrlEncodedContent(keyValues)
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
                        await (await response.ToGenericHttpResponseAsync(contentAsString:true)).ToStringBuilderAsync(sb);
                        Log.Trace(sb.ToString(), messageId);
                    }

                    if (!response.IsSuccessStatusCode)
                        return loggedFailedOutcome(response, await isPendingUserCodeAsync(response, cts.Token));

#if NET5_0_OR_GREATER
                    await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
#else
                    using var stream = await response.Content.ReadAsStreamAsync();
#endif
                    var body = await JsonSerializer.DeserializeAsync<DeviceCodeAuthorizationResponseBody>(
                        stream, 
                        cancellationToken: cts.Token);

                    var parseOutcome = DeviceCodeResponse.TryParse(body!); 
                    if (parseOutcome)
                        return Outcome<DeviceCodeResponse>.Success(parseOutcome.Value!);
                    
                    Log.Error(parseOutcome.Exception!);
                    return Outcome<DeviceCodeResponse>.Fail(parseOutcome.Exception!);

                }
                catch (Exception ex)
                {
                    ex = new Exception("Device Code grant failure when polling for token (see inner)", ex);
                    Log.Error(ex);
                    return Outcome<DeviceCodeResponse>.Fail(ex);
                }
            }
            
            Outcome<DeviceCodeResponse> loggedFailedOutcome(HttpResponseMessage response, bool isPendingUserCodeAsync)
            {
                var outcome = Outcome<DeviceCodeResponse>.Fail(
                    new HttpServerException(response), 
                    DeviceCodeResponse.ForPendingCodeVerification(isPendingUserCodeAsync)); 
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

                message.AppendLine("Client credentials failure (state dump to follow if DEBUG log level is enabled)");
                if (Log?.IsEnabled(LogRank.Debug) ?? false)
                {
                    var dump = new StateDump().WithStackTrace();
                    dump.AddAsync(TetraPakConfig, "AuthConfig");
                    dump.AddAsync(clientCredentials, "Credentials");
                    message.AppendLine(dump.ToString());
                }
                Log.Error(outcome.Exception!, message.ToString(), messageId);
                return Outcome<DeviceCodeResponse>.Fail(outcome.Exception!);
            }
        }

//         static async Task<bool> isPendingUserCodeAsync(Outcome<DeviceCodeResponse> response, CancellationToken cancellationToken) obsolete
//         {
//             if (response.Exception is not HttpServerException { StatusCode: HttpStatusCode.BadRequest } serverException) 
//                 return false;
//
//             // return await isPendingUserCodeAsync(serverException.Response!, cancellationToken);
//             
// #if NET5_0_OR_GREATER
//             await using var stream = await serverException.Response!.Content.ReadAsStreamAsync(cancellationToken);
// #else                
//             using var stream = await serverException.Response!.Content.ReadAsStreamAsync();
// #endif
//             try
//             {
//                 var dict = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream, cancellationToken: cancellationToken);
//                 return dict!.TryGetValue("detail", out var value) &&
//                        value.StartsWith("Pending", StringComparison.InvariantCultureIgnoreCase);
//             }
//             catch
//             {
//                 return false;
//             }
//         }
        
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


        Task<Outcome<Credentials>> getCredentialsAsync()
        {
            if (string.IsNullOrWhiteSpace(TetraPakConfig.ClientId))
                return Task.FromResult(Outcome<Credentials>.Fail(
                    new HttpServerConfigurationException("Client credentials have not been provisioned")));

            return Task.FromResult(Outcome<Credentials>.Success(
                new BasicAuthCredentials(TetraPakConfig.ClientId!, TetraPakConfig.ClientSecret!)));
        }

        public TetraPakDeviceCodeGrantService(
            ITetraPakConfiguration tetraPakConfig, 
            IHttpClientProvider httpClientProvider,
            ITimeLimitedRepositories? cache = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(tetraPakConfig, httpClientProvider, cache, log, httpContextAccessor)
        {
        }
    }
}