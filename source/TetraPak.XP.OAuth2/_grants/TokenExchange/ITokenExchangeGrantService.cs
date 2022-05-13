using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2.TokenExchange
{
    public interface ITokenExchangeGrantService : IGrantService
    {
        // todo
        Task<Outcome<Grant>> AcquireTokenAsync(ActorToken subjectToken, GrantOptions options);
    }
}