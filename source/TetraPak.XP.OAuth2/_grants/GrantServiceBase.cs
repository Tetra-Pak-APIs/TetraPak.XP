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
using TetraPak.XP.OAuth2.ClientCredentials;
using TetraPak.XP.OAuth2.Refresh;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2
{
    public abstract class GrantServiceBase
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
        protected async Task<Outcome<ActorToken>> IsRefreshingGrantsAsync(AuthContext context)
        {
            if (RefreshTokenGrantService is null || !context.Options.IsRefreshAllowed)
                return Outcome<ActorToken>.Fail("Refresh not allowed or no refresh token service available");
                    
            return await GetCachedRefreshTokenAsync(context);
        }
        
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
            return Task.FromResult(Outcome<Credentials>.Success(new BasicAuthCredentials(identity, secret!)));
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

        // protected async Task DeleteCachedGrantAsync(AuthContext context) obsolete
        // {
        //     if (!IsGrantCachingEnabled(context))
        //         return;
        //
        //     var appCredentialsOutcome = await GetAppCredentialsAsync(context);
        //     if (!appCredentialsOutcome)
        //         return;
        //
        //     var key = appCredentialsOutcome.Value!.Identity;
        //     var cacheRepository = context.GetGrantCacheRepository();
        //     await TokenCache!.DeleteAsync(key, cacheRepository);
        // }

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

//         bool tryGetGrantCacheKey(
//             AuthContext context,
// #if NET5_0_OR_GREATER
//             [NotNullWhen(true)]
// #endif            
//             out string? key)
//         {
//             var clientId = context.Configuration.ClientId;
//             key = null;
//             if (_appCredentialsDelegate is null)
//             {
//                 key = context.Configuration.ClientId!;
//                 return !string.IsNullOrEmpty(key);
//             }
//
//             var outcome = _appCredentialsDelegate.GetGrantCacheKey(context);
//             if (!outcome)
//                 return false;
//
//             key = string.IsNullOrEmpty(outcome.Value!) || outcome.Value == clientId
//                 ? clientId
//                 : new DynamicPath(clientId, key).StringValue;
//             return !string.IsNullOrEmpty(key);
//         }


//         bool tryGetRefreshTokenCacheKey( obsolete
//             AuthContext context,
// #if NET5_0_OR_GREATER
//             [NotNullWhen(true)]
// #endif
//             out string? key)
//         {
//             var clientId = context.Configuration.ClientId;
//             key = null;
//             if (_appCredentialsDelegate is null)
//             {
//                 key = context.Configuration.ClientId!;
//                 return !string.IsNullOrEmpty(key);
//             }
//
//             key = string.IsNullOrEmpty(outcome.Value!) || outcome.Value == clientId
//                 ? clientId
//                 : new DynamicPath(clientId, key).StringValue;
//             return !string.IsNullOrEmpty(key);
//         }

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