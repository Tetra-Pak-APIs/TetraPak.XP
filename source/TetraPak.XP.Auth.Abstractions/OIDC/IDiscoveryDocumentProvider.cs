using System.Threading.Tasks;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    public interface IDiscoveryDocumentProvider
    {
        Task<Outcome<DiscoveryDocument>> GetDiscoveryDocument(ActorToken? idToken);
    }
}