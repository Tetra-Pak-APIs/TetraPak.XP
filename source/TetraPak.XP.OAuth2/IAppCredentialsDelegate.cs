using Microsoft.Extensions.Configuration;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2
{
    public interface IAppCredentialsDelegate
    {
        Outcome<Credentials> GetAppCredentials(IConfiguration configuration, AuthContext context);
    }
}