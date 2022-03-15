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

namespace TetraPak.XP.OAuth2.ClientCredentials
{
    /// <summary>
    ///   A default service to support the client credentials grant type.
    /// </summary>
    sealed class TetraPakClientCredentialsGrantService : GrantServiceBase, IClientCredentialsGrantService
    {
        protected override GrantType GetGrantType() => GrantType.ClientCredentials;
        
        /// <inheritdoc />
        public async Task<Outcome<Grant>> AcquireTokenAsync(GrantOptions options)
        {
            // todo Consider breaking up this method (it's too big)
            // todo Honor the GrantOptions.Flags value (silent/forced request etc.)
            var messageId = GetMessageId();

            var authContextOutcome = TetraPakConfig.GetAuthContext(GrantType.ClientCredentials, options);
            if (!authContextOutcome)
                return Outcome<Grant>.Fail(authContextOutcome.Exception!);
            var ctx = authContextOutcome.Value!;

            var appCredentialsOutcome = await GetAppCredentialsAsync(ctx);
            if (!appCredentialsOutcome)
                return Outcome<Grant>.Fail(appCredentialsOutcome.Exception!);
            var appCredentials = appCredentialsOutcome.Value!;
            
            
            var tokenIssuerUri = ctx.Configuration.TokenIssuerUri;
            if (string.IsNullOrWhiteSpace(tokenIssuerUri))
                return ctx.Configuration.MissingConfigurationOutcome<Grant>(nameof(IAuthConfiguration.TokenIssuerUri));
            
            var ctSource = options.CancellationTokenSource ?? new CancellationTokenSource();
            try
            {
                var basicAuthCredentials = ValidateBasicAuthCredentials(appCredentials);
                var cachedOutcome = await GetCachedGrantAsync(ctx);
                if (cachedOutcome)
                {
                    var cachedGrant = cachedOutcome.Value!;
                    if (cachedGrant.Expires <= DateTime.UtcNow)
                        return cachedOutcome;
                }
                
                var clientOutcome = await GetHttpClientAsync();
                if (!clientOutcome)
                    return Outcome<Grant>.Fail(
                        new HttpServerConfigurationException(
                            "Client credentials service failed to obtain a HTTP client (see inner exception)", 
                            clientOutcome.Exception));
                
                using var client = clientOutcome.Value!;
                client.DefaultRequestHeaders.Authorization = basicAuthCredentials.ToAuthenticationHeaderValue();
                var formsValues = new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials"
                };
                if (options.Scope is { })
                {
                    formsValues.Add("scope", options.Scope.Items.ConcatCollection(" "));
                }

                var keyValues = formsValues.Select(kvp 
                    => new KeyValuePair<string?, string?>(kvp.Key, kvp.Value));

                
                
                var request = new HttpRequestMessage(HttpMethod.Post, tokenIssuerUri)
                {
                    Content = new FormUrlEncodedContent(keyValues)
                };
                var sb = Log?.IsEnabled(LogRank.Trace) ?? false
                    ? await (await request.ToGenericHttpRequestAsync()).ToStringBuilderAsync(
                        new StringBuilder(), 
                        () => TraceHttpRequestOptions.Default(messageId)
                            .WithInitiator(this, HttpDirection.Out)
                            .WithDefaultHeaders(client.DefaultRequestHeaders))
                    : null;

                var response = await client.SendAsync(request, ctSource.Token);
                
                if (sb is { })
                {
                    sb.AppendLine();
                    await (await response.ToGenericHttpResponseAsync()).ToStringBuilderAsync(sb);
                    Log.Trace(sb.ToString(), messageId);
                }
                
                if (!response.IsSuccessStatusCode)
                    return loggedFailedOutcome(response);

#if NET5_0_OR_GREATER
                var stream = await response.Content.ReadAsStreamAsync(ctSource.Token);
#else
                var stream = await response.Content.ReadAsStreamAsync();
#endif
                var responseBody =
                    await JsonSerializer.DeserializeAsync<ClientCredentialsResponseBody>(
                        stream,
                        cancellationToken: ctSource.Token);

                var outcome = ClientCredentialsResponse.TryParse(responseBody!);
                if (outcome)
                {
                    var grant = outcome.Value!.ToGrant();
                    await CacheGrantAsync(ctx, grant);
                }

                var g = outcome.Value!;
                return Outcome<Grant>.Success(
                    new Grant().ForClientCredentials(g.AccessToken, DateTime.UtcNow.Add(g.ExpiresIn))); // todo consider subtracting a bit from the 'expires' timespan
            }
            catch (TaskCanceledException ex)
            {
                Log.Warning(ex.Message);
                return Outcome<Grant>.Fail(ex);
            }
            catch (Exception ex)
            {
                ex = new Exception($"Failed to acquire token using client credentials. {ex.Message}", ex);
                Log.Error(ex);
                return Outcome<Grant>.Fail(ex);
            }
            
            Outcome<Grant> loggedFailedOutcome(HttpResponseMessage response)
            {
                var ex = new HttpServerException(response); 
                if (Log is null)
                    return Outcome<Grant>.Fail(ex);

                // var messageId = _tetraPakConfig.AmbientData.GetMessageId(true);
                var message = new StringBuilder();
                message.AppendLine("Client credentials failure (state dump to follow if DEBUG log level is enabled)");
                if (Log.IsEnabled(LogRank.Debug))
                {
                    var dump = new StateDump().WithStackTrace();
                    dump.AddAsync(TetraPakConfig, "AuthConfig");
                    dump.AddAsync(appCredentials, "Credentials");
                    message.AppendLine(dump.ToString());
                }
                Log.Error(ex, message.ToString(), messageId);
                return Outcome<Grant>.Fail(ex);
            }
        }

        /// <summary>
        ///   Initializes the <see cref="TetraPakClientCredentialsGrantService"/>.
        /// </summary>
        /// <param name="tetraPakConfig">
        ///   The Tetra Pak integration configuration.
        /// </param>
        /// <param name="httpClientProvider">
        ///   A HttpClient factory.
        /// </param>
        /// <param name="refreshTokenGrantService">
        ///   Enables the OAuth Refresh Grant flow. 
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
        public TetraPakClientCredentialsGrantService(
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