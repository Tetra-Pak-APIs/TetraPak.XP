using System.Threading.Tasks;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    public interface IDiscoveryDocumentProvider
    {
        Task<Outcome<DiscoveryDocument>> GetDiscoveryDocumentAsync(IStringValue? idToken, GrantOptions? options = null);
    }
}