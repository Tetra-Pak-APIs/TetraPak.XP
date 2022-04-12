using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2.TokenExchange
{
    public sealed class TetraPakTokenExchangeGrantService : GrantServiceBase, ITokenExchangeGrantService
    {
        protected override GrantType GetGrantType() => GrantType.TokenExchange;

        // todo
        public Task<Outcome<Grant>> AcquireTokenAsync(ActorToken token, GrantOptions options)
        {
            throw new System.NotImplementedException();
        }

        public TetraPakTokenExchangeGrantService(
            IHttpClientProvider httpClientProvider,
            ITetraPakConfiguration? tetraPakConfig = null, 
            ITokenCache? tokenCache = null,
            IAppCredentialsDelegate? appCredentialsDelegate = null,
            ILog? log = null,
            IHttpContextAccessor? httpContextAccessor = null)
        : base(httpClientProvider, tetraPakConfig, null, tokenCache, appCredentialsDelegate, log, httpContextAccessor)
        {
        }
    }
}