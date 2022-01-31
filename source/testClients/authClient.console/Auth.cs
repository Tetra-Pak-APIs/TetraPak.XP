using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.OIDC;
using TetraPak.XP.Desktop;
using TetraPak.XP.Logging;
using TetraPak.XP.SimpleDI;

namespace authClient.console
{
    public class Auth
    {
        readonly IAuthenticator _authenticator;
        readonly ILog? _log;

        public async Task NewTokenAsync()
        {
            writeToLog(await _authenticator.GetAccessTokenAsync(false));
        }

        public async Task SilentTokenAsync()
        {
            writeToLog(await _authenticator.GetAccessTokenSilentlyAsync());
        }

        void writeToLog(Outcome<AuthResult> outcome)
        {
            if (!outcome)
            {
                if (!string.IsNullOrWhiteSpace(outcome.Message))
                {
                    _log.Warning(outcome.Message);
                }
                else if (outcome.Exception is { })
                {
                    _log.Warning(outcome.Exception.Message);
                }
                else
                {
                    _log.Warning("Authorization failed with no message");
                }
                return;
            }

            var sb = new StringBuilder("SUCCESS! ");
            sb.Append("access_token=");
            sb.Append(outcome.Value!.AccessToken);
            if (!string.IsNullOrWhiteSpace(outcome.Value.IdToken))
            {
                sb.Append("id_token=");
                sb.Append(outcome.Value.IdToken);
            }
            if (!string.IsNullOrWhiteSpace(outcome.Value.RefreshToken))
            {
                sb.Append("refresh_token=");
                sb.Append(outcome.Value.RefreshToken);
            }
            _log.Info(sb.ToString());
        }

        public Auth(ILog log)
        {
            var c = XpServices.GetServiceCollection();
            var authApp = (AuthApplication)"DEV; hiJSBHQzj0v5k58j2SYTT8h54iIO8OIr; http://localhost:42444/auth";
            c.AddSingleton(_ => log);
            c.AddSingleton(_ => authApp);
            c.AddTetraPakOidcAuthentication<InteractiveDesktopBrowser>(authApp);
            var services = c.BuildXpServiceProvider();
            _log = services.GetService<ILog>();
            _authenticator = services.GetRequiredService<IAuthenticator>();
        }
    }
}