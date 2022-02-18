using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.ClientCredentials;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth
{
    public abstract class GrantServiceBase
    {
        protected ITetraPakConfiguration TetraPakConfig { get; }
        
        IHttpClientProvider HttpClientProvider { get; }
        
        IHttpContextAccessor? HttpContextAccessor { get; }
        
        protected ILog? Log  { get; }

        readonly ITimeLimitedRepositories? _cache;

        HttpContext? HttpContext => HttpContextAccessor?.HttpContext;
        
        /// <summary>
        ///   Obtains a message id used for tracking a request/response (mainly for diagnostics purposes).
        /// </summary>
        protected LogMessageId? GetMessageId() => HttpContext?.Request.GetMessageId(TetraPakConfig);

        protected Task<Outcome<HttpClient>> GetHttpClientAsync() => HttpClientProvider.GetHttpClientAsync();

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
            if (_cache is null) 
                return;

            await _cache.CreateOrUpdateAsync(
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
        ///     The credentials used to acquire the response.
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="ClientCredentialsResponse"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        protected async Task<Outcome<Grant>> GetCachedResponse(string cacheRepositoryName, Credentials credentials)
        {
            if (_cache is null)
                return Outcome<Grant>.Fail(new Exception("No cached token"));

            var cachedOutcome = await _cache.ReadAsync<Grant>(
                credentials.Identity,
                cacheRepositoryName);

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
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            TetraPakConfig = tetraPakConfig ?? throw new ArgumentNullException(nameof(tetraPakConfig));
            HttpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
            HttpContextAccessor = httpContextAccessor;
            Log = log;
            _cache = cache;
            
        }
    }
}