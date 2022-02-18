using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.OIDC;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;
using TetraPak.XP.Logging;

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

        void writeToLog(Outcome<Grant> outcome)
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

            var sb = new StringBuilder();
            sb.AppendLine("SUCCESS!");
            var grant = outcome.Value!;
            foreach (var token in grant.Tokens!)
            {
                sb.Append($"  {token.Role}={token.Token}");
            }
            _log.Information(sb.ToString());
        }

        public Auth(ILog log)
        {
            var c = XpServices.GetServiceCollection();
            var authApp = (AuthApplication)"DEV; hiJSBHQzj0v5k58j2SYTT8h54iIO8OIr; http://localhost:42444/auth";
            c.AddSingleton(_ => log);
            c.AddSingleton(_ => authApp);
            c.AddTetraPakOidcAuthentication<DesktopLoopbackBrowser>(authApp);
            var services = c.BuildXpServiceProvider();
            _log = services.GetService<ILog>();
            _authenticator = services.GetRequiredService<IAuthenticator>();
        }
    }
}