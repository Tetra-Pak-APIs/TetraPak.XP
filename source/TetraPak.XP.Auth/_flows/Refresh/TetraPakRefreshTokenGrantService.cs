using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging;
using TetraPk.XP.Web.Http;

namespace TetraPak.XP.Auth.Refresh
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
            
            var clientIdOutcome = await TetraPakConfig.GetClientIdAsync(authContext);
            if (!clientIdOutcome)
                return Outcome<string>.Fail(clientIdOutcome.Exception!);
            var clientId = clientIdOutcome.Value!;
            sb.Append($"&client_id={clientId}");

            return Outcome<string>.Success(sb.ToString());
        }
        
        public TetraPakRefreshTokenGrantService(
            ITetraPakConfiguration tetraPakConfig, 
            IHttpClientProvider httpClientProvider,
            ITokenCache? tokenCache = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(tetraPakConfig, httpClientProvider, null, tokenCache, log, httpContextAccessor)
        {
        }
    }
}