using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;
using TetraPak.XP.Identity;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.OAuth2;
using TetraPak.XP.OAuth2.AuthCode;
using TetraPak.XP.OAuth2.ClientCredentials;
using TetraPak.XP.OAuth2.DeviceCode;
using TetraPak.XP.OAuth2.OIDC;
using TetraPak.XP.OAuth2.TokenExchange;
using TetraPak.XP.Web.Services;
using UserInformation = TetraPak.XP.Identity.UserInformation;

namespace authClient.console
{
    sealed class Auth
    {
        readonly ILog? _log;
        readonly IServiceProvider? _serviceProvider;
        Grant? _lastGrant;

        public Task AcquireTokenAsync(GrantType grantType, CancellationTokenSource cts, bool forced = false)
        {
            return Task.Run(async () =>
            {
                var options = forced 
                    ? GrantOptions.Forced(cts)
                    : GrantOptions.Silent(cts);
                var provider = _serviceProvider ?? throw new Exception("No service provider!");
                switch (grantType)
                {
                    case GrantType.OIDC:
                        var ac = provider.GetRequiredService<IAuthorizationCodeGrantService>();
                        Console.WriteLine();
                        Console.WriteLine("OIDC grant requested ...");
                        onOutcome(await ac.AcquireTokenAsync(options)); 
                        break;
                
                    case GrantType.CC:
                        var cc = provider.GetRequiredService<IClientCredentialsGrantService>();
                        Console.WriteLine();
                        Console.WriteLine("Client Credentials grant requested ...");
                        onOutcome(await cc.AcquireTokenAsync(options));
                        break;
                
                    case GrantType.DC:
                        var dc = provider.GetRequiredService<IDeviceCodeGrantService>();
                        Console.WriteLine();
                        Console.WriteLine("Device Code grant requested ...");
                        onOutcome(await dc.AcquireTokenAsync(options, requestUSerCodeVerification));
                        break;
                
                    case GrantType.TX:
                        Console.WriteLine();
                        if (_lastGrant?.AccessToken is null)
                        {
                            Console.WriteLine("Token Exchange requires an existing token. Please execute other grant first");
                            break;
                        }
                        var tx = provider.GetRequiredService<ITokenExchangeGrantService>();
                        Console.WriteLine("Token Exchange grant requested ...");
                        onOutcome(await tx.AcquireTokenAsync(_lastGrant.AccessToken, options));
                        break;
                
                    default:
                        throw new ArgumentOutOfRangeException(nameof(grantType), grantType, null);
                }
            });
        }

        public async Task GetUserInformationAsync(CancellationTokenSource cts)
        {
            if (_lastGrant?.AccessToken is null)
            {
                Console.WriteLine("Please obtain a grant and try again!");
                return;
            }
            
            var provider = _serviceProvider ?? throw new Exception("No service provider!");
            var ui = provider.GetRequiredService<IUserInformationService>();
            onOutcome(await ui.GetUserInformationAsync(_lastGrant, GrantOptions.Default()));
        }

        static void requestUSerCodeVerification(VerificationArgs args) => Console.WriteLine($"Please very code '{args.UserCode}' on: {args.VerificationUri} ...");

        void onOutcome(Outcome<Grant> outcome)
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

            _lastGrant = outcome.Value;
            
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

        void onOutcome(Outcome<UserInformation> outcome)
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
                    _log.Warning("Could not get user information (no error message available)");
                }
                return;
            }
            
            var sb = new StringBuilder();
            sb.AppendLine("SUCCESS!");
            var information = outcome.Value!;
            foreach (var pair in information.ToDictionary())
            {
                sb.AppendLine($"  {pair.Key} = {pair.Value}");
            }
            _log.Information(sb.ToString);
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
            var info = args.BuildTetraPakDesktopHost(collection =>
            {
                collection
                    .AddTetraPakWebServices()
                    .AddDesktopTokenCache() 
                    .AddAppCredentialsDelegate<CustomAppCredentialsDelegate>()
                    .AddTetraPakOidcGrant()
                    .AddTetraPakClientCredentialsGrant()
                    .AddTetraPakDeviceCodeGrant()
                    .AddTetraPakTokenExchangeGrant()
                    .AddTetraPakUserInformation()
                    // just a very basic log (abstracted by the ILog interface, you can use something else here, like NLog, SemiLog, Log4Net or whatever)
                    .AddSingleton(p => new LogBase(p.GetService<IConfiguration>()).WithConsoleLogging());
            });
            _serviceProvider = info.ServiceServiceCollection.BuildXpServiceProvider();
            _log = _serviceProvider.GetService<ILog>();
        }
    }
}