using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.ClientCredentials;
using TetraPak.XP.Auth.DeviceCode;
using TetraPak.XP.Auth.OIDC;
using TetraPak.XP.Configuration;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;
using TetraPak.XP.Logging;
using IConfiguration = TetraPak.XP.Configuration.IConfiguration;

namespace authClient.console
{
    public class Auth
    {
        readonly IAuthenticator _authenticator;
        readonly ILog? _log;
        

        public void NewTokenAsync(GrantType grantType, CancellationTokenSource cancellationTokenSource)
        {
            Task.Run(async () =>
            {
                switch (grantType)
                {
                    case GrantType.CC:
                        var cc = XpServices.GetRequired<IClientCredentialsGrantService>();
                        Console.WriteLine();
                        Console.WriteLine("Client Credentials grant requested ...");
                        writeToLog(await cc.AcquireTokenAsync(cancellationTokenSource));
                        break;
                
                    case GrantType.DC:
                        var dc = XpServices.GetRequired<IDeviceCodeGrantService>();
                        Console.WriteLine();
                        Console.WriteLine("Device Code grant requested ...");
                        writeToLog(await dc.AcquireTokenAsync(askUserToVerifyCode, cancellationTokenSource));
                        break;
                
                    case GrantType.OIDC:
                        Console.WriteLine();
                        Console.WriteLine("OIDC grant requested ...");
                        writeToLog(await _authenticator.GetAccessTokenAsync(false, cancellationTokenSource)); // todo Rewrite OIDC to be a service instead (like with CC, above)
                        break;
                
                    default:
                        throw new ArgumentOutOfRangeException(nameof(grantType), grantType, null);
                }
            });
            
        }

        static void askUserToVerifyCode(VerificationArgs args) => Console.WriteLine($"Please very code '{args.UserCode}' on: {args.VerificationUri} ...");

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
        
        void writeToLog(Outcome<DeviceCodeResponse> outcome)
        {
            if (!outcome)
            {
                if (outcome.Exception is TaskCanceledException)
                    return; // already logged as an ERROR
                
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
                    _log.Warning("Device code grant failed with no message");
                }
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("SUCCESS!");
            sb.Append("  access_token=");
            sb.AppendLine(outcome.Value!.AccessToken);
            _log.Information(sb.ToString());
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

        public Auth()
        {
            var authApp = (AuthApplication)"DEV; hiJSBHQzj0v5k58j2SYTT8h54iIO8OIr; http://localhost:42444/auth";
            var collection = new ServiceCollection();
            var services = XpServices.BuildFor().Desktop().UseServiceCollection(collection)
                .AddSingleton( p =>
                {
                    var rank = resolveLogRank(p, LogRank.Information);
                    var log = new BasicLog { Rank = rank } .WithConsoleLogging();
                    return log;

                })
                .AddSingleton(_ => authApp)
                .AddTetraPakOidcAuthentication<DesktopLoopbackBrowser>(authApp)
                .AddTetraPakClientCredentialsAuthentication()
                .AddTetraPakDeviceCodeAuthentication()
                .BuildXpServiceProvider();
            _log = services.GetService<ILog>();
            _authenticator = services.GetRequiredService<IAuthenticator>();
        }

        static LogRank resolveLogRank(IServiceProvider p, LogRank useDefault)
        {
            var config = p.GetRequiredService<IConfiguration>();
            var logLevelSection = config.GetSectionAsync(new ConfigPath(new[] { "Logging", "LogLevel" })).Result;
            if (logLevelSection is null)
                return useDefault;

            var s = logLevelSection.GetAsync<string>("Default").Result;
            if (string.IsNullOrEmpty(s))
                return useDefault;
            
            return s.TryParseEnum(typeof(LogRank), out var obj) && obj is LogRank logRank
                ? logRank
                : useDefault;

        }
    }
}