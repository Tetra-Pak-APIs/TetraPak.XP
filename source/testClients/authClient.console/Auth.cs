using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;
using TetraPak.XP.Logging;
using TetraPak.XP.OAuth2;
using TetraPak.XP.OAuth2.AuthCode;
using TetraPak.XP.OAuth2.ClientCredentials;
using TetraPak.XP.OAuth2.DeviceCode;
using TetraPak.XP.OAuth2.OIDC;
using TetraPak.XP.Web.Services;

namespace authClient.console
{
    public class Auth
    {
        ILog? _log;
        IServiceProvider? _serviceProvider;

        public Task AcquireTokenAsync(GrantType grantType, CancellationTokenSource cts, bool silent)
        {
            return Task.Run(async () =>
            {
                var options = silent 
                    ? GrantOptions.Silent(cts) 
                    : GrantOptions.Forced(cts);
                var provider = _serviceProvider ?? throw new Exception("No service provider!");
                switch (grantType)
                {
                    case GrantType.OIDC:
                        var ac = provider.GetRequiredService<IAuthorizationCodeGrantService>();
                        Console.WriteLine();
                        Console.WriteLine("OIDC grant requested ...");
                        writeToLog(await ac.AcquireTokenAsync(options)); 
                        break;
                
                    case GrantType.CC:
                        var cc = provider.GetRequiredService<IClientCredentialsGrantService>();
                        Console.WriteLine();
                        Console.WriteLine("Client Credentials grant requested ...");
                        writeToLog(await cc.AcquireTokenAsync(options));
                        break;
                
                    case GrantType.DC:
                        var dc = provider.GetRequiredService<IDeviceCodeGrantService>();
                        Console.WriteLine();
                        Console.WriteLine("Device Code grant requested ...");
                        writeToLog(await dc.AcquireTokenAsync(options, requestUSerCodeVerification));
                        break;
                
                    default:
                        throw new ArgumentOutOfRangeException(nameof(grantType), grantType, null);
                }
            });
        }

        static void requestUSerCodeVerification(VerificationArgs args) => Console.WriteLine($"Please very code '{args.UserCode}' on: {args.VerificationUri} ...");

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
            sb.AppendLine("Token(s):");
            foreach (var token in grant.Tokens!)
            {
                sb.AppendLine($"  {token.Role}={token.Token}");
            }

            sb.AppendLine($"Scope={grant.Scope}");
            _log.Information(sb.ToString());
        }
        
        internal async Task ClearCachedGrantsAsync()
        {
            var p = _serviceProvider ?? throw new Exception("No service provider!");
            await p.GetRequiredService<IAuthorizationCodeGrantService>().ClearCachedGrantsAsync();
            await p.GetRequiredService<IClientCredentialsGrantService>().ClearCachedGrantsAsync();
            await p.GetRequiredService<IDeviceCodeGrantService>().ClearCachedGrantsAsync();
            // todo clear Token Exchange service cached grants
        }

        internal async Task ClearCachedRefreshTokensAsync()
        {
            var p = _serviceProvider ?? throw new Exception("No service provider!");
            await p.GetRequiredService<IAuthorizationCodeGrantService>().ClearCachedRefreshTokensAsync();
            await p.GetRequiredService<IClientCredentialsGrantService>().ClearCachedRefreshTokensAsync();
            await p.GetRequiredService<IDeviceCodeGrantService>().ClearCachedRefreshTokensAsync();
            // todo clear Token Exchange service refresh tokens
        }

        public Auth(string[] args)
        {
            Configure.InsertValueDelegate(new ConfigurationVariablesDelegate());
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(collection =>
                {
                    _serviceProvider = XpServices
                        .BuildFor().Desktop()
                        .AddServiceCollection(collection)
                        .AddTetraPakConfiguration()
                        .AddTetraPakWebServices()
                        .AddTokenCaching() 
                         .AddSingleton( p =>
                         {
                             var rank = resolveLogRank(p, LogRank.Information);
                             var log = new BasicLog { Rank = rank } .WithConsoleLogging();
                             return log;
                         })
                        .AddAppCredentialsDelegate<CustomAppCredentialsDelegate>()
                        .AddTetraPakOidcGrant()
                        .AddTetraPakClientCredentialsGrant()
                        .AddTetraPakDeviceCodeGrant()
                        .BuildXpServices();
                    _log = _serviceProvider.GetService<ILog>();
                })
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables();
                })
                .ConfigureAppConfiguration((_, builder) => builder.Build())
                .Build();
        }
        
        static LogRank resolveLogRank(IServiceProvider p, LogRank useDefault)
        {
            var config = p.GetRequiredService<IConfiguration>();
            var logLevelSection = config.GetSubSection(new ConfigPath(new[] { "Logging", "LogLevel" }));
            if (logLevelSection is null)
                return useDefault;

            var s = logLevelSection.GetNamed<string>("Default");
            if (string.IsNullOrEmpty(s))
                return useDefault;
            
            return s.TryParseEnum(typeof(LogRank), out var obj) && obj is LogRank logRank
                ? logRank
                : useDefault;
        }
    }
}