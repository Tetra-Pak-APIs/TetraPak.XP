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
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Http.Debugging;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth.ClientCredentials
{
    /// <summary>
    ///   A default service to support the client credentials grant type.
    /// </summary>
    public sealed class TetraPakClientCredentialsGrantService : GrantServiceBase, IClientCredentialsGrantService
    {
        // readonly ITetraPakConfiguration _tpConfig; obsolete
        // readonly IHttpClientProvider _httpClientProvider;
        // readonly ITimeLimitedRepositories? _cache;
        // readonly IHttpContextAccessor? _httpContextAccessor;
        // readonly ILog? _log;

        const string CacheRepository = CacheRepositories.Tokens.ClientCredentials;

        // HttpContext? HttpContext => HttpContextAccessor?.HttpContext; obsolete

        /// <inheritdoc />
        public async Task<Outcome<ClientCredentialsResponse>> AcquireTokenAsync(
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
                        return Outcome<ClientCredentialsResponse>.Fail(ccOutcome.Exception!);

                    clientCredentials = ccOutcome.Value!;
                }

                var basicAuthCredentials = ValidateBasicAuthCredentials(clientCredentials);
                var cachedOutcome = forceAuthorization 
                        ? Outcome<ClientCredentialsResponse>.Fail(new Exception("nisse")) // nisse Write proper error message
                        : await GetCachedResponse<ClientCredentialsResponse>(CacheRepository, basicAuthCredentials);
                if (cachedOutcome)
                {
                    var cachedResponse = cachedOutcome.Value!;
                    if (cachedResponse.ExpiresIn.Subtract(TimeSpan.FromSeconds(2)) > TimeSpan.Zero)
                        return cachedOutcome;
                }
                
                var clientOutcome = await GetHttpClientAsync();
                if (!clientOutcome)
                    return Outcome<ClientCredentialsResponse>.Fail(
                        new HttpServerConfigurationException(
                            "Client credentials service failed to obtain a HTTP client (see inner exception)", 
                            clientOutcome.Exception));
                
                using var client = clientOutcome.Value!;
                client.DefaultRequestHeaders.Authorization = basicAuthCredentials.ToAuthenticationHeaderValue();
                var formsValues = new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials"
                };
                if (scope is { })
                {
                    formsValues.Add("scope", scope.Items.ConcatCollection(" "));
                }

                var keyValues = formsValues.Select(kvp 
                    => new KeyValuePair<string?, string?>(kvp.Key, kvp.Value));

                var tokenIssuerUrl = await TetraPakConfig.GetTokenIssuerUrlAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, tokenIssuerUrl)
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
                    await JsonSerializer.DeserializeAsync<ClientCredentialsResponseBody>(
                        stream,
                        cancellationToken: ct);

                var outcome = ClientCredentialsResponse.TryParse(responseBody!);
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
                return Outcome<ClientCredentialsResponse>.Fail(ex);
            }
            
            Outcome<ClientCredentialsResponse> loggedFailedOutcome(HttpResponseMessage response, LogMessageId? messageId)
            {
                var ex = new HttpServerException(response); 
                if (Log is null)
                    return Outcome<ClientCredentialsResponse>.Fail(ex);

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
                return Outcome<ClientCredentialsResponse>.Fail(ex);
            }
        }
        
        /// <summary>
        ///   This virtual asynchronous method is automatically invoked when <see cref="AcquireTokenAsync"/>
        ///   needs client credentials. 
        /// </summary>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="Credentials"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        Task<Outcome<Credentials>> getCredentialsAsync()
        {
            if (string.IsNullOrWhiteSpace(TetraPakConfig.ClientId))
                return Task.FromResult(Outcome<Credentials>.Fail(
                    new HttpServerConfigurationException("Client credentials have not been provisioned")));

            return Task.FromResult(Outcome<Credentials>.Success(
                new BasicAuthCredentials(TetraPakConfig.ClientId!, TetraPakConfig.ClientSecret!)));
        }

        // /// <summary>
        // ///   Invoked from <see cref="AcquireTokenAsync"/> when to try fetching a cached auth response.  obsolete
        // /// </summary>
        // /// <param name="credentials">
        // ///     The credentials used to acquire the response.
        // /// </param>
        // /// <returns>
        // ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        // ///   a <see cref="ClientCredentialsResponse"/> or, on failure, an <see cref="Exception"/>.
        // /// </returns>
        // protected async Task<Outcome<ClientCredentialsResponse>> GetCachedResponse(Credentials credentials)
        // {
        //     if (_cache is null)
        //         return Outcome<ClientCredentialsResponse>.Fail(new Exception("No cached token"));
        //
        //     var cachedOutcome = await _cache.ReadAsync<ClientCredentialsResponse>(
        //         credentials.Identity,
        //         CacheRepository);
        //
        //     if (!cachedOutcome)
        //         return cachedOutcome;
        //
        //     var remainingLifeSpan = cachedOutcome.GetRemainingLifespan();
        //     return cachedOutcome
        //         ? Outcome<ClientCredentialsResponse>.Success(cachedOutcome.Value!.Clone(remainingLifeSpan))
        //         : cachedOutcome;
        // }

        // /// <summary>
        // ///   Invoked from <see cref="AcquireTokenAsync"/> when receiving a successful auth response.  
        // /// </summary>
        // /// <param name="credentials">
        // ///     The credentials used to acquire the response.
        // /// </param>
        // /// <param name="response">
        // ///     The response to be cached.
        // /// </param>
        // /// <returns>
        // ///   The <paramref name="response"/> value.
        // /// </returns>
        // protected async Task CacheResponseAsync(Credentials credentials, ClientCredentialsResponse response)
        // {
        //     if (_cache is null) 
        //         return;
        //
        //     await _cache.CreateOrUpdateAsync(
        //         response,
        //         credentials.Identity,
        //         CacheRepository,
        //         response.ExpiresIn);
        // }
        
        // static BasicAuthCredentials OnValidateBasicAuthCredentials(Credentials credentials) obsolete
        // {
        //     if (string.IsNullOrWhiteSpace(credentials.Identity) || string.IsNullOrWhiteSpace(credentials.Secret))
        //         throw new InvalidOperationException("Invalid credentials. Please specify client id and secret");
        //
        //     return new BasicAuthCredentials(credentials.Identity, credentials.Secret!);
        // }

        // /// <summary>
        // ///   This virtual asynchronous method is automatically invoked when <see cref="AcquireTokenAsync"/> obsolete
        // ///   needs client credentials. 
        // /// </summary>
        // /// <returns>
        // ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        // ///   a <see cref="Credentials"/> or, on failure, an <see cref="Exception"/>.
        // /// </returns>
        // protected virtual Task<Outcome<Credentials>> OnGetCredentialsAsync()
        // {
        //     if (string.IsNullOrWhiteSpace(_tetraPakConfig.ClientId))
        //         return Task.FromResult(Outcome<Credentials>.Fail(
        //             new HttpServerConfigurationException("Client credentials have not been provisioned")));
        //
        //     return Task.FromResult(Outcome<Credentials>.Success(
        //         new BasicAuthCredentials(_tetraPakConfig.ClientId!, _tetraPakConfig.ClientSecret!)));
        // }

        /// <summary>
        ///   Initializes the <see cref="TetraPakClientCredentialsGrantService"/>.
        /// </summary>
        /// <param name="tetraPakConfig">
        ///   The Tetra Pak integration configuration.
        /// </param>
        /// <param name="httpClientProvider">
        ///   A HttpClient factory.
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logger provider.   
        /// </param>
        /// <param name="httpContextAccessor">
        ///   Provides access to the current request/response <see cref="HttpContext"/>. 
        /// </param>
        /// <param name="cache">
        ///   (optional)<br/>
        ///   A cache to reduce traffic and improve performance
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Any parameter was <c>null</c>.
        /// </exception>
        public TetraPakClientCredentialsGrantService(
            ITetraPakConfiguration tetraPakConfig, 
            IHttpClientProvider httpClientProvider,
            ITimeLimitedRepositories? cache = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(tetraPakConfig, httpClientProvider, cache, log, httpContextAccessor)
        {
            // _tpConfig = tetraPakConfig; obsolete
            // _httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
            // _httpContextAccessor = httpContextAccessor;
            // _log = log;
            // _cache = cache;
        }
    }
}