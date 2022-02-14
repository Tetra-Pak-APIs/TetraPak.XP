using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth
{
    public abstract class GrantServiceBase
    {
        readonly ITetraPakConfiguration _tpConfig;
        readonly IHttpClientProvider _httpClientProvider;
        readonly IHttpContextAccessor? _httpContextAccessor;
        readonly ILog? _log;
        readonly ITimeLimitedRepositories? _cache;
        
        /// <summary>
        ///   This virtual asynchronous method is automatically invoked when <see cref="AcquireTokenAsync"/>
        ///   needs client credentials. 
        /// </summary>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="Credentials"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        protected virtual Task<Outcome<Credentials>> OnGetCredentialsAsync()
        {
            if (string.IsNullOrWhiteSpace(_tpConfig.ClientId))
                return Task.FromResult(Outcome<Credentials>.Fail(
                    new HttpServerConfigurationException("Client credentials have not been provisioned")));

            return Task.FromResult(Outcome<Credentials>.Success(
                new BasicAuthCredentials(_tpConfig.ClientId!, _tpConfig.ClientSecret!)));
        }
        
        protected BasicAuthCredentials ValidateBasicAuthCredentials(Credentials credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.Identity) || string.IsNullOrWhiteSpace(credentials.Secret))
                throw new InvalidOperationException("Invalid credentials. Please specify client id and secret");

            return new BasicAuthCredentials(credentials.Identity, credentials.Secret!);
        }

        public GrantServiceBase(
            ITetraPakConfiguration tpConfig, 
            IHttpClientProvider httpClientProvider,
            ITimeLimitedRepositories? cache = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _tpConfig = tpConfig;
            _httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
            _httpContextAccessor = httpContextAccessor;
            _log = log;
            _cache = cache;
            
        }
    }
}