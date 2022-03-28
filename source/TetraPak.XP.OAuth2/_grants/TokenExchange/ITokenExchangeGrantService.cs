using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2.TokenExchange
{
    public interface ITokenExchangeGrantService
    {
        // todo
        Task<Outcome<Grant>> AcquireTokenAsync(ActorToken token, GrantOptions options);
    }
}