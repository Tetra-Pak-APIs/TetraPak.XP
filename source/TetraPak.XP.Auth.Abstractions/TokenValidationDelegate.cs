using System.Threading.Tasks;

namespace TetraPak.XP.Auth.Abstractions
{
    delegate Task<Outcome<ActorToken>> TokenValidationDelegate(ActorToken token);
}