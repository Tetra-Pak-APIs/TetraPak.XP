using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Caching;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;
using TetraPak.XP.OAuth2.AuthCode;
using TetraPak.XP.OAuth2.ClientCredentials;
using TetraPak.XP.OAuth2.Refresh;
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Services;

namespace TetraPak.XP.OAuth2
{
    public abstract class GrantServiceBase : IGrantService
    {
        readonly IAppCredentialsDelegate? _appCredentialsDelegate;

        /// <summary>
        ///   The Tetra Pak configuration object.
        /// </summary>
        protected ITetraPakConfiguration TetraPakConfig { get; }
        
        IHttpClientProvider HttpClientProvider { get; }
        
        IHttpContextAccessor? HttpContextAccessor { get; }
        
        /// <summary>
        ///   A Refresh Grant service.
        /// </summary>
        protected IRefreshTokenGrantService? RefreshTokenGrantService { get; }
        
        /// <summary>
        ///   A logger provider.
        /// </summary>
        protected ILog? Log  { get; }

        protected abstract GrantType GetGrantType();

        /// <summary>
        ///   Examines passed state and returns a value indicating whether the current <see cref="Grant"/> request
        ///   can be resolved from an earlier, cached, <see cref="Grant"/>.
        /// </summary>
        /// <param name="context">
        ///   describe an auth request context.
        /// </param>
        /// <param name="isWritingToCache">
        ///   Specifies whether the query implicates a write operation (to the cache).
        /// </param>
        /// <returns>
        ///   <c>true</c> if the Refresh Grant is currently available and possible; otherwise <c>false</c>.
        /// </returns>
        bool IsGrantCachingEnabled(AuthContext context, bool isWritingToCache) 
            => 
            TokenCache is {} && TetraPakConfig.IsCaching && (context.Options.IsCachingAllowed || isWritingToCache);

        /// <summary>
        ///   Examines passed state and returns a value indicating whether the Refresh Grant flow is
        ///   currently available. 
        /// </summary>
        /// <param name="context">
        ///   describe an auth request context.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the Refresh Grant is currently available and possible; otherwise <c>false</c>.
        /// </returns>
        protected bool IsRefreshingGrants(AuthContext context) => RefreshTokenGrantService is {} && context.Options.IsRefreshAllowed;

        /// <summary>
        ///   A (secure) cache to be used fo caching <see cref="Grant"/>s or individual tokens.
        /// </summary>
        protected ITokenCache? TokenCache { get; }

        HttpContext? HttpContext => HttpContextAccessor?.HttpContext;
        
        /// <summary>
        ///   Obtains a message id used for tracking a request/response (mainly for diagnostics purposes).
        /// </summary>
        protected LogMessageId? GetMessageId() => HttpContext?.Request.GetMessageId(TetraPakConfig);

        /// <summary>
        ///   Constructs and returns a <see cref="HttpClient"/>. 
        /// </summary>
        /// <returns></returns>
        protected Task<Outcome<HttpClient>> GetHttpClientAsync() => HttpClientProvider.GetHttpClientAsync();
        
        /// <summary>
        ///   Gets application credentials (the client id and, if applicable, client secret)
        ///   for a specified auth context. 
        /// </summary>
        /// <param name="context">
        ///   Details the current auth context.
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="Credentials"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        protected Task<Outcome<Credentials>> GetAppCredentialsAsync(AuthContext context)
        {
            if (_appCredentialsDelegate is { })
            {
                var outcome = _appCredentialsDelegate.GetAppCredentials(context);
                if (outcome)
                    return Task.FromResult(outcome);
            }
            
            var identity = context.Configuration.ClientId;
            if (string.IsNullOrWhiteSpace(identity))
                return Task.FromResult(Outcome<Credentials>.Fail(
                    new ConfigurationException("Client credentials have not been provisioned")));

            var secret = context.Configuration.ClientSecret;
            return Task.FromResult(Outcome<Credentials>.Success(new BasicAuthCredentials(identity!, secret!)));
        }

        /// <summary>
        ///   Caches a grant.  
        /// </summary>
        /// <param name="context">
        ///   describe an auth request context.
        /// </param>
        /// <param name="grant">
        ///     The grant to be cached.
        /// </param>
        /// <typeparam name="T">
        ///   The type of grant to be cached (must derive from <see cref="Grant"/>).
        /// </typeparam>
        /// <returns>
        ///   The <paramref name="grant"/> value.
        /// </returns>
        protected async Task CacheGrantAsync<T>(AuthContext context, T grant)
        where T : Grant
        {
            if (!IsGrantCachingEnabled(context, true))
                return;

            var appCredentialsOutcome = await GetAppCredentialsAsync(context);
            if (!appCredentialsOutcome)
                return;

            var key = appCredentialsOutcome.Value!.Identity;
            await TokenCache!.CreateOrUpdateAsync(
                grant,
                key,
                context.GetGrantCacheRepository(),
                spawnTimeUtc: DateTime.UtcNow);
        }

        /// <summary>
        ///   Retrieves cached <see cref="Grant"/>.  
        /// </summary>
        /// <param name="context">
        ///   describe an auth request context.
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="ClientCredentialsResponse"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        protected async Task<Outcome<Grant>> GetCachedGrantAsync(AuthContext context)
        {
            if (!IsGrantCachingEnabled(context, false))
                return Outcome<Grant>.Fail("Token caching is unavailable or not allowed");

            var appCredentialsOutcome = await GetAppCredentialsAsync(context);
            if (!appCredentialsOutcome)
                return Outcome<Grant>.Fail("Could not resolve app credentials");

            var key = appCredentialsOutcome.Value!.Identity;
            var cacheRepository = context.GetGrantCacheRepository();
            var cachedOutcome = await TokenCache!.ReadAsync<Grant>(
                key,
                cacheRepository, 
                context.CancellationToken);

            if (!cachedOutcome)
                return cachedOutcome;

            var remainingLifeSpan = cachedOutcome.GetRemainingLifespan();
            return cachedOutcome
                ? Outcome<Grant>.Success(cachedOutcome.Value!.Clone(remainingLifeSpan))
                : cachedOutcome;
        }

        protected async Task DeleteCachedGrantAsync(AuthContext context) 
        {
            if (!IsGrantCachingEnabled(context, true))
                return;
        
            var appCredentialsOutcome = await GetAppCredentialsAsync(context);
            if (!appCredentialsOutcome)
                return;
        
            var key = appCredentialsOutcome.Value!.Identity;
            var cacheRepository = context.GetGrantCacheRepository();
            await TokenCache!.DeleteAsync(key, cacheRepository);
        }

        /// <summary>
        ///   Caches a token.  
        /// </summary>
        /// <param name="context">
        ///   describe an auth request context.
        /// </param>
        /// <param name="refreshToken">
        ///     The grant to be cached.
        /// </param>
        /// <typeparam name="T">
        ///   The type of token to be cached (must derive from <see cref="ActorToken"/>).
        /// </typeparam>
        /// <returns>
        ///   The <paramref name="refreshToken"/> value.
        /// </returns>
        protected async Task CacheRefreshTokenAsync<T>(AuthContext context, T refreshToken)
        where T : ActorToken
        {
            if (!IsGrantCachingEnabled(context, true))
                return;
            
            var appCredentialsOutcome = await GetAppCredentialsAsync(context);
            if (!appCredentialsOutcome)
                return;

            var key = appCredentialsOutcome.Value!.Identity;
            await TokenCache!.CreateOrUpdateAsync(
                refreshToken,
                key,
                CacheRepositories.Tokens.Refresh,
                spawnTimeUtc: DateTime.UtcNow);
        }
        
        protected async Task<Outcome<ActorToken>> GetCachedRefreshTokenAsync( 
            AuthContext context,
            CancellationToken? cancellationToken = null)
        {
            if (TokenCache is null)
                return Outcome<ActorToken>.Fail("No token cache is available");
        
            var appCredentialsOutcome = await GetAppCredentialsAsync(context);
            if (!appCredentialsOutcome)
                return Outcome<ActorToken>.Fail("Could not resolve app credentials");

            var key = appCredentialsOutcome.Value!.Identity;
            var cachedOutcome = await TokenCache.ReadAsync<ActorToken>(
                key,
                CacheRepositories.Tokens.Refresh,
                cancellationToken);
        
            if (!cachedOutcome)
                return cachedOutcome;
        
            return cachedOutcome
                ? Outcome<ActorToken>.Success(cachedOutcome.Value!.Clone<ActorToken>())
                : cachedOutcome;
        }

        protected async Task DeleteCachedRefreshTokenAsync(AuthContext context) 
        {
            if (!IsGrantCachingEnabled(context, true))
                return;
        
            var appCredentialsOutcome = await GetAppCredentialsAsync(context);
            if (!appCredentialsOutcome)
                return;
        
            var key = appCredentialsOutcome.Value!.Identity;
            var cacheRepository = context.GetRefreshTokenCacheRepository();
            await TokenCache!.DeleteAsync(key, cacheRepository);
        }

        public async Task ClearCachedGrantsAsync()
        {
            var ctxOutcome = TetraPakConfig.GetAuthContext(GetGrantType(), new GrantOptions());
            if (!ctxOutcome)
                return;

            var ctx = ctxOutcome.Value!;
            await DeleteCachedGrantAsync(ctx);
        }

        public async Task ClearCachedRefreshTokensAsync()
        {
            var ctxOutcome = TetraPakConfig.GetAuthContext(GetGrantType(), new GrantOptions());
            if (!ctxOutcome)
                return;

            var ctx = ctxOutcome.Value!;
            await DeleteCachedRefreshTokenAsync(ctx);
        }

        /// <summary>
        ///   Validates <see cref="Credentials"/> to be used as <see cref="BasicAuthCredentials"/>.
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns>
        ///   
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   The <paramref name="credentials"/> lacks either the <see cref="Credentials.Identity"/> or
        ///   <see cref="Credentials.Secret"/> elements.
        /// </exception>
        protected BasicAuthCredentials ValidateBasicAuthCredentials(Credentials credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.Identity) || string.IsNullOrWhiteSpace(credentials.Secret))
                throw new InvalidOperationException("Invalid credentials. Please specify client id and secret");
            
            return credentials is BasicAuthCredentials bac
                ? bac
                : new BasicAuthCredentials(credentials.Identity, credentials.Secret!);
        }

        /// <summary>
        ///   Initializes the grant service.
        /// </summary>
        /// <param name="tetraPakConfig">
        ///   The Tetra Pak configuration object.
        /// </param>
        /// <param name="httpClientProvider">
        ///   A HTTP client provider, required for communicating with Tetra Pak.
        /// </param>
        /// <param name="refreshTokenGrantService">
        ///   Enables the OAuth Refresh Grant flow. 
        /// </param>
        /// <param name="tokenCache"></param>
        /// <param name="appCredentialsDelegate">
        ///   (optional)<br/>
        ///   A delegate to provide custom logic when acquiring app credentials (client id/secret).
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A log provider.
        /// </param>
        /// <param name="httpContextAccessor">
        ///   A <see cref="IHttpContextAccessor"/> used by the <see cref="GetMessageId"/> method.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Any of the compulsory arguments where unassigned.
        /// </exception>
        protected GrantServiceBase(ITetraPakConfiguration tetraPakConfig,
            IHttpClientProvider httpClientProvider,
            IRefreshTokenGrantService? refreshTokenGrantService = null,
            ITokenCache? tokenCache = null,
            IAppCredentialsDelegate? appCredentialsDelegate = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            TetraPakConfig = tetraPakConfig ?? throw new ArgumentNullException(nameof(tetraPakConfig));
            HttpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
            RefreshTokenGrantService = refreshTokenGrantService;
            TokenCache = tokenCache;
            _appCredentialsDelegate = appCredentialsDelegate;
            Log = log;
            HttpContextAccessor = httpContextAccessor;
        }
    }
}