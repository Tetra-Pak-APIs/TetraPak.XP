using System;
using System.Collections.Generic;
using System.Net.Http;
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
        async Task<Outcome<Grant>> IRefreshTokenGrantService.AcquireTokenAsync(
            ActorToken refreshToken,
            GrantOptions options)
        {
            Log.Debug("---- START - Tetra Pak Refresh Token Flow ----");
            var authContextOutcome = TetraPakConfig.GetAuthContext(GrantType.AC, options);
            if (!authContextOutcome)
                return Outcome<Grant>.Fail(authContextOutcome.Exception!);

            var ctx = authContextOutcome.Value!;
            var messageId = GetMessageId();
                        
            var appCredentialsOutcome = await GetAppCredentialsAsync(ctx);
            if (!appCredentialsOutcome)
                return Outcome<Grant>.Fail(appCredentialsOutcome.Exception!);
            
            var appCredentials = appCredentialsOutcome.Value!;
            var clientId = appCredentials.Identity; 

            var tokenIssuerUriString = ctx.Configuration.TokenIssuerUri;
            if (string.IsNullOrWhiteSpace(tokenIssuerUriString))
                return ctx.Configuration.MissingConfigurationOutcome<Grant>(nameof(AuthContext.Configuration.TokenIssuerUri));

            
            
            throw new NotImplementedException();
        }

        static Outcome<FormUrlEncodedContent> makeRefreshTokenBody(
            string refreshToken, 
            string? clientId, 
            AuthContext authContext)
        {
            var dict = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };
            
            if (clientId is null)
                return Outcome<FormUrlEncodedContent>.Success(new FormUrlEncodedContent(dict!));

            var conf = authContext.Configuration;
            if (string.IsNullOrWhiteSpace(clientId))
                return conf.MissingConfigurationOutcome<FormUrlEncodedContent>(nameof(AuthContext.Configuration.ClientId));

            dict["client_id"] = clientId;
            return Outcome<FormUrlEncodedContent>.Success(new FormUrlEncodedContent(dict!));
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