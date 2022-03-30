using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2.TokenExchange
{
    sealed class TetraPakTokenExchangeGrantService : GrantServiceBase, ITokenExchangeGrantService
    {
        protected override GrantType GetGrantType() => GrantType.TokenExchange;

        // todo
        public Task<Outcome<Grant>> AcquireTokenAsync(ActorToken token, GrantOptions options)
        {
            throw new System.NotImplementedException();
        }

        public TetraPakTokenExchangeGrantService(
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