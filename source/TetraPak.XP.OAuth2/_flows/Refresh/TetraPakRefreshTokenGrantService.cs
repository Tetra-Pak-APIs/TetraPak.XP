using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2.Refresh
{
    class TetraPakRefreshTokenGrantService : GrantServiceBase, IRefreshTokenGrantService
    {
        /// <inheritdoc />
        Task<Outcome<Grant>> IRefreshTokenGrantService.AcquireTokenAsync(
            ActorToken refreshToken,
            GrantOptions options)
        {
            Log.Debug("---- START - Tetra Pak Refresh Token Flow ----");
            throw new NotImplementedException();
        }

        async Task<Outcome<string>> makeRefreshTokenBodyAsync(string refreshToken, bool includeClientId, AuthContext authContext)
        {
            var sb = new StringBuilder();
            sb.Append("grant_type=refresh_token");
            sb.Append($"&refresh_token={refreshToken}");

            if (!includeClientId)
                return Outcome<string>.Success(sb.ToString());

            var conf = authContext.Configuration;
            var clientId = conf.ClientId;
            if (string.IsNullOrWhiteSpace(clientId))
                return AuthConfiguration.MissingConfigurationOutcome<string>(conf, nameof(AuthContext.Configuration.ClientId));
            
            sb.Append($"&client_id={clientId}");

            return Outcome<string>.Success(sb.ToString());
        }
        
        public TetraPakRefreshTokenGrantService(
            ITetraPakConfiguration tetraPakConfig, 
            IHttpClientProvider httpClientProvider,
            ITokenCache? tokenCache = null,
            IAppCredentialsDelegate? appCredentialsDelegate = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(tetraPakConfig, httpClientProvider, null, tokenCache, appCredentialsDelegate, log, httpContextAccessor)
        {
        }
    }
}