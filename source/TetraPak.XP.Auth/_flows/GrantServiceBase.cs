using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.ClientCredentials;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth
{
    public abstract class GrantServiceBase
    {
        // /// <summary>
        // ///   Gets a default cache key (the client Id).
        // /// </summary>
        // protected string TokenCacheKey => $"{TetraPakConfig.Authority!.Host}::{TetraPakConfig.ClientId}"; obsolete
        
        protected ITetraPakConfiguration TetraPakConfig { get; }
        
        IHttpClientProvider HttpClientProvider { get; }
        
        IHttpContextAccessor? HttpContextAccessor { get; }
        
        protected ILog? Log  { get; }

        protected bool IsCachingGrants(GrantOptions options) => TetraPakConfig.IsCaching && options.IsCachingAllowed && TokenCache is {};

        protected ITimeLimitedRepositories? Cache { get; }  
        
        protected ITokenCache? TokenCache { get; }

        HttpContext? HttpContext => HttpContextAccessor?.HttpContext;
        
        /// <summary>
        ///   Obtains a message id used for tracking a request/response (mainly for diagnostics purposes).
        /// </summary>
        protected LogMessageId? GetMessageId() => HttpContext?.Request.GetMessageId(TetraPakConfig);

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
        /// <param name="credentials">
        ///     The credentials used to acquire the response.
        /// </param>
        /// <param name="response">
        ///     The response to be cached.
        /// </param>
        /// <returns>
        ///   The <paramref name="response"/> value.
        /// </returns>
        protected async Task CacheResponseAsync<T>(string cacheRepositoryName, Credentials credentials, T response)
        where T : GrantResponse
        {
            if (Cache is null) 
                return;

            await Cache.CreateOrUpdateAsync(
                response,
                credentials.Identity,
                cacheRepositoryName,
                response.ExpiresIn);
        }

        /// <summary>
        ///   Retrieves cached <see cref="Credentials"/>.  
        /// </summary>
        /// <param name="cacheRepositoryName">
        ///   The name of the repository caching the response.
        /// </param>
        /// <param name="credentials">
        ///   The credentials used to acquire the response.
        /// </param>
        /// <param name="cancellationToken">
        ///  (optional)<br/>
        ///   Allows canceling the request.
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="ClientCredentialsResponse"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        protected async Task<Outcome<Grant>> GetCachedResponseAsync(
            string cacheRepositoryName, 
            Credentials credentials,
            CancellationToken? cancellationToken = null)
        {
            if (TokenCache is null)
                return Outcome<Grant>.Fail("No token cache is available");
                
            var cachedOutcome = await TokenCache.ReadAsync<Grant>(
                credentials.Identity,
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
        /// <param name="cache">
        ///   (optional)<br/>
        ///   A time-limited repositories provider, used to cache grants.
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
        protected GrantServiceBase(
            ITetraPakConfiguration tetraPakConfig, 
            IHttpClientProvider httpClientProvider,
            ITimeLimitedRepositories? cache = null,
            ITokenCache? tokenCache = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            TetraPakConfig = tetraPakConfig ?? throw new ArgumentNullException(nameof(tetraPakConfig));
            HttpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
            HttpContextAccessor = httpContextAccessor;
            Log = log;
            Cache = cache;
            TokenCache = tokenCache;
        }

    }
}