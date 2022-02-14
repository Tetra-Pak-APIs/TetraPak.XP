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
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging;
using TetraPak.XP.Web.Debugging;
using TetraPk.XP.Web.Http;
using TetraPk.XP.Web.Http.Debugging;

namespace TetraPak.XP.Auth.DeviceCode
{
    public class TetraPakDeviceCodeGrantService : GrantServiceBase, IDeviceCodeGrantService
    {
        const string CacheRepository = CacheRepositories.Tokens.DeviceCodeCredentials;

        public async Task<Outcome<DeviceCodeResponse>> AcquireTokenAsync(
            CancellationToken? cancellationToken = null, 
            Credentials? clientCredentials = null,
            MultiStringValue? scope = null, 
            bool forceAuthorization = false)
        {
             // todo Consider breaking up this method (it's too big) 
            try
            {
                var ct = cancellationToken ?? CancellationToken.None;
                if (clientCredentials is null)
                {
                    var ccOutcome = await getCredentialsAsync();
                    if (!ccOutcome)
                        return Outcome<DeviceCodeResponse>.Fail(ccOutcome.Exception!);

                    clientCredentials = ccOutcome.Value!;
                }

                var basicAuthCredentials = ValidateBasicAuthCredentials(clientCredentials);
                var cachedOutcome = forceAuthorization 
                        ? Outcome<DeviceCodeResponse>.Fail(new Exception("nisse")) // nisse Write proper error message
                        : await GetCachedResponse<DeviceCodeResponse>(CacheRepository, basicAuthCredentials);
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
                var messageId = GetMessageId();
                var sb = Log?.IsEnabled(LogRank.Trace) ?? false
                    ? await (await request.ToGenericHttpRequestAsync()).ToStringBuilderAsync(
                        new StringBuilder(), 
                        () => TraceHttpRequestOptions.Default(messageId)
                            .WithInitiator(this, HttpDirection.Out)
                            .WithDefaultHeaders(client.DefaultRequestHeaders))
                    : null;

                var response = await client.SendAsync(request, ct);
                
                if (sb is { })
                {
                    sb.AppendLine();
                    await (await response.ToGenericHttpResponseAsync()).ToStringBuilderAsync(sb);
                    Log.Trace(sb.ToString(), messageId);
                }
                
                if (!response.IsSuccessStatusCode)
                    return loggedFailedOutcome(response, messageId);

#if NET5_0_OR_GREATER
                var stream = await response.Content.ReadAsStreamAsync(ct);
#else
                var stream = await response.Content.ReadAsStreamAsync();
#endif
                var responseBody =
                    await JsonSerializer.DeserializeAsync<DeviceCodeResponseBody>(
                        stream,
                        cancellationToken: ct);

                var outcome = DeviceCodeResponse.TryParse(responseBody!);
                if (outcome)
                {
                    await CacheResponseAsync(CacheRepository, basicAuthCredentials, outcome.Value!);
                }

                return outcome;
            }
            catch (Exception ex)
            {
                ex = new Exception($"Failed to acquire token using client credentials. {ex.Message}", ex);
                Log.Error(ex);
                return Outcome<DeviceCodeResponse>.Fail(ex);
            }
            
            Outcome<DeviceCodeResponse> loggedFailedOutcome(HttpResponseMessage response, LogMessageId? messageId)
            {
                var ex = new HttpServerException(response); 
                if (Log is null)
                    return Outcome<DeviceCodeResponse>.Fail(ex);

                // var messageId = _tetraPakConfig.AmbientData.GetMessageId(true);
                var message = new StringBuilder();
                message.AppendLine("Client credentials failure (state dump to follow if DEBUG log level is enabled)");
                if (Log.IsEnabled(LogRank.Debug))
                {
                    var dump = new StateDump().WithStackTrace();
                    dump.AddAsync(TetraPakConfig, "AuthConfig");
                    dump.AddAsync(clientCredentials, "Credentials");
                    message.AppendLine(dump.ToString());
                }
                Log.Error(ex, message.ToString(), messageId);
                return Outcome<DeviceCodeResponse>.Fail(ex);
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
            // _tetraPakConfig = tetraPakConfig; obsolete
            // _httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
            // _httpContextAccessor = httpContextAccessor;
            // _log = log;
            // _cache = cache;
        }
    }
}