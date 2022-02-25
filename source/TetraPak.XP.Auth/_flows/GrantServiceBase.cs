using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.ClientCredentials;
using TetraPak.XP.Auth.Refresh;
using TetraPak.XP.Caching;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth
{
    public abstract class GrantServiceBase
    {
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
        /// <param name="options">
        ///   Specifies options for the <see cref="Grant"/> request.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the Refresh Grant is currently available and possible; otherwise <c>false</c>.
        /// </returns>
        protected bool IsCachingGrants(GrantOptions options) => TetraPakConfig.IsCaching && options.IsCachingAllowed && TokenCache is {};

        /// <summary>
        ///   Examines passed state and returns a value indicating whether the Refresh Grant flow is
        ///   currently available. 
        /// </summary>
        /// <param name="refreshToken">
        ///   An available refresh token.
        /// </param>
        /// <param name="options">
        ///   Specifies options for the <see cref="Grant"/> request.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the Refresh Grant is currently available and possible; otherwise <c>false</c>.
        /// </returns>
        protected bool IsRefreshingGrants(ActorToken? refreshToken, GrantOptions options)
            => RefreshTokenGrantService is { } && options.IsRefreshAllowed && !string.IsNullOrEmpty(refreshToken);

        // protected ITimeLimitedRepositories? Cache { get; }  
        
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
        ///   Gets the configures application credentials (the client id and, if applicable, client secret). 
        /// </summary>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="Credentials"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        protected Task<Outcome<Credentials>> GetAppCredentialsAsync()
        {
            if (string.IsNullOrWhiteSpace(TetraPakConfig.ClientId))
                return Task.FromResult(Outcome<Credentials>.Fail(
                    new ConfigurationException("Client credentials have not been provisioned")));

            return Task.FromResult(Outcome<Credentials>.Success(
                new BasicAuthCredentials(TetraPakConfig.ClientId!, TetraPakConfig.ClientSecret!)));
        }

        /// <summary>
        ///   Caches <see cref="Credentials"/>.  
        /// </summary>
        /// <param name="cacheRepositoryName">
        ///   The name of the repository to cache the response.
        /// </param>
        /// <param name="key">
        ///   The key used to cache the <see cref="Grant"/>.
        /// </param>
        /// <param name="grant">
        ///     The grant to be cached.
        /// </param>
        /// <returns>
        ///   The <paramref name="grant"/> value.
        /// </returns>
        protected Task CacheGrantAsync<T>(string cacheRepositoryName, string key, T grant)
        where T : Grant
        {
            if (TokenCache is null) 
                return Task.CompletedTask;

            return TokenCache.CreateOrUpdateAsync(
                grant,
                key,
                cacheRepositoryName,
                spawnTimeUtc: DateTime.UtcNow);
        }

        /// <summary>
        ///   Retrieves cached <see cref="Credentials"/>.  
        /// </summary>
        /// <param name="cacheRepositoryName">
        ///   The name of the repository caching the response.
        /// </param>
        /// <param name="key">
        ///   The key used to cache the <see cref="Grant"/>.
        /// </param>
        /// <param name="cancellationToken">
        ///  (optional)<br/>
        ///   Allows canceling the request.
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="ClientCredentialsResponse"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        protected async Task<Outcome<Grant>> GetCachedGrantAsync(
            string cacheRepositoryName,
            string key,
            CancellationToken? cancellationToken = null)
        {
            if (TokenCache is null)
                return Outcome<Grant>.Fail("No token cache is available");
                
            var cachedOutcome = await TokenCache.ReadAsync<Grant>(
                key,
                cacheRepositoryName, 
                cancellationToken);

            if (!cachedOutcome)
                return cachedOutcome;

            var remainingLifeSpan = cachedOutcome.GetRemainingLifespan();
            return cachedOutcome
                ? Outcome<Grant>.Success(cachedOutcome.Value!.Clone(remainingLifeSpan))
                : cachedOutcome;
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
            IRefreshTokenGrantService? refreshTokenGrantService,
            // ITimeLimitedRepositories? cache = null, obsolete
            ITokenCache? tokenCache = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            TetraPakConfig = tetraPakConfig ?? throw new ArgumentNullException(nameof(tetraPakConfig));
            HttpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
            RefreshTokenGrantService = refreshTokenGrantService;
            HttpContextAccessor = httpContextAccessor;
            Log = log;
            // Cache = cache; obsolete
            TokenCache = tokenCache;
        }
    }
}