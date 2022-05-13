using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.Diagnostics;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.OAuth2.Refresh;
using TetraPak.XP.Web.Abstractions;
using TetraPak.XP.Web.Abstractions.Debugging;
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Services;

namespace TetraPak.XP.OAuth2.TokenExchange
{
    public sealed class TetraPakTokenExchangeGrantService : GrantServiceBase, ITokenExchangeGrantService
    {
        readonly IDiscoveryDocumentProvider _discoveryDocumentProvider;
        protected override GrantType GetGrantType() => GrantType.TokenExchange;

        // todo
        public async Task<Outcome<Grant>> AcquireTokenAsync(
            ActorToken subjectToken, 
            GrantOptions options)
        {
            if (subjectToken.IsSystemIdentityToken())
                return Outcome<Grant>.Fail(this.TokenExchangeNotSupportedForSystemIdentity());

            var messageId = GetMessageId();
            var authContextOutcome = TetraPakConfig.GetAuthContext(GrantType.TokenExchange, options);
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
                    return cachedOutcome;
                
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
                            "Token Exchange grant service failed to obtain a HTTP client (see inner exception)",
                            clientOutcome.Exception));

                using var client = clientOutcome.Value!;
                if (clientCredentials is not BasicAuthCredentials basicAuthCredentials)
                {
                    basicAuthCredentials = new BasicAuthCredentials(
                        clientCredentials.Identity, 
                        clientCredentials.Secret!); // todo Do we need ro check if secret was assigned?
                }
                client.DefaultRequestHeaders.Authorization = basicAuthCredentials.ToAuthenticationHeaderValue();
                var discoOutcome = await _discoveryDocumentProvider.GetDiscoveryDocumentAsync(subjectToken);
                if (!discoOutcome)
                    return Outcome<Grant>.Fail(
                        HttpServerException.InternalServerError(
                            "Token Exchange service failed to obtain an OIDC discovery document", 
                            clientOutcome.Exception));

                var discoveryDocument = discoOutcome.Value!;
                var form = new TokenExchangeArgs(
                    clientCredentials, 
                    subjectToken, 
                    "urn:ietf:params:oauth:token-type:id_token");
                var request = new HttpRequestMessage(HttpMethod.Post, discoveryDocument.TokenEndpoint)
                {
#if NET5_0_OR_GREATER                    
                    Content = new FormUrlEncodedContent(form.ToDictionary()!)
#else
                    Content = new FormUrlEncodedContent(form.ToDictionary())
#endif                    
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
                    response = await client.SendAsync(request, CancellationToken);
                    if (IsCancellationRequested)
                        return Outcome<Grant>.Cancel("Token Exchange grant request was cancelled");
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
                    return await loggedFailedOutcomeAsync(response);

#if NET5_0_OR_GREATER
                var stream = await response.Content.ReadAsStreamAsync(CancellationToken);
#else
                var stream = await response.Content.ReadAsStreamAsync();
#endif

                try
                {
                    var responseBody = await JsonSerializer.DeserializeAsync<TokenExchangeResponseBody>(
                        stream,
                        cancellationToken: CancellationToken);
                    var outcome = TokenExchangeResponse.TryParse(responseBody!);
                    var grant = outcome.Value!.ToGrant();
                    if (outcome)
                    {
                        await CacheGrantAsync(authContext, grant);
                    }
                    
                    var g = outcome.Value!;
                    return Outcome<Grant>.Success(
                        new Grant().ForClientCredentials(g.AccessToken, XpDateTime.UtcNow.Add(g.ExpiresIn))); // todo consider subtracting a bit from the 'expires' timespan
                }
                catch (Exception ex)
                {
                    ex = new Exception("Token Exchange failed to parse result", ex);
                    return Outcome<Grant>.Fail(ex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            
            async Task<Outcome<Grant>> loggedFailedOutcomeAsync(HttpResponseMessage response)
            {
                var ex = new HttpServerException(response); 
                if (Log is null)
                    return Outcome<Grant>.Fail(ex);

                var message = new StringBuilder();
                message.AppendLine("Token Exchange failure (state dump to follow if DEBUG log level is enabled)");
                if (Log.IsEnabled(LogRank.Debug))
                {
                    var dump = new StateDump().WithStackTrace();
                    await dump.AddAsync(TetraPakConfig, "AuthConfig");
                    await dump.AddAsync(clientCredentials, "Credentials");
                    message.AppendLine(dump.ToString());
                }
                Log.Error(ex, message.ToString(), messageId);
                return Outcome<Grant>.Fail(ex);
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

        public TetraPakTokenExchangeGrantService(
            IHttpClientProvider httpClientProvider,
            IDiscoveryDocumentProvider discoveryDocumentProvider,
            ITetraPakConfiguration? tetraPakConfig = null, 
            IRefreshTokenGrantService? refreshTokenGrantService = null,
                ITokenCache? tokenCache = null,
            IAppCredentialsDelegate? appCredentialsDelegate = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(httpClientProvider, tetraPakConfig, refreshTokenGrantService, tokenCache, appCredentialsDelegate, log, httpContextAccessor)
        {
            _discoveryDocumentProvider = discoveryDocumentProvider;
        }
    }
}