using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
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

        public async Task NewTokenAsync(GrantType grantType)
        {
            switch (grantType)
            {
                case GrantType.CC:
                    var cc = XpServices.GetRequired<IClientCredentialsGrantService>();
                    writeToLog(await cc.AcquireTokenAsync());
                    break;
                
                case GrantType.OIDC:
                    writeToLog(await _authenticator.GetAccessTokenAsync(false)); // todo Rewrite OIDC to be a service instead (like with CC, above)
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(grantType), grantType, null);
            }
        }

        public async Task SilentTokenAsync()
        {
            writeToLog(await _authenticator.GetAccessTokenSilentlyAsync());
        }
        
        void writeToLog(Outcome<ClientCredentialsResponse> outcome)
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
                    _log.Warning("Client Credentials authorization failed with no message");
                }
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("SUCCESS!");
            sb.Append("  access_token=");
            sb.AppendLine(outcome.Value!.AccessToken);
            _log.Information(sb.ToString());
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

            var sb = new StringBuilder();
            sb.AppendLine("SUCCESS!");
            sb.Append("  access_token=");
            sb.AppendLine(outcome.Value!.AccessToken);
            if (!string.IsNullOrWhiteSpace(outcome.Value.IdToken))
            {
                sb.Append("  id_token=");
                sb.AppendLine(outcome.Value.IdToken);
                
                
            }
            if (!string.IsNullOrWhiteSpace(outcome.Value.RefreshToken))
            {
                sb.Append("  refresh_token=");
                sb.AppendLine(outcome.Value.RefreshToken);
            }
            _log.Information(sb.ToString());
        }

        public Auth(ILog log)
        {
            var authApp = (AuthApplication)"DEV; hiJSBHQzj0v5k58j2SYTT8h54iIO8OIr; http://localhost:42444/auth";
            var collection = new ServiceCollection();
            var services = XpServices.BuildFor().Desktop().UseServiceCollection(collection)
                .AddSingleton(_ => log)
                .AddSingleton(_ => authApp)
                .AddTetraPakOidcAuthentication<DesktopLoopbackBrowser>(authApp)
                .AddTetraPakClientCredentialsAuthentication()
                .BuildXpServiceProvider();
            _log = services.GetService<ILog>();
            _authenticator = services.GetRequiredService<IAuthenticator>();
        }
    }
}