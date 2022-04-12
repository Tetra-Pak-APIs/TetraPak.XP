using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.CommandLine;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;
using TetraPak.XP.OAuth2.DeviceCode;
using TetraPak.XP.Web.Http;

namespace DeviceCodeConsoleApp;

static class DeviceCodeGrant
{
    internal static Task<Outcome<Grant>> AcquireDeviceCodeTokenAsync(this string[] args, Action<VerificationArgs> verificationUriHandler)
    {
        if (!args.TryGetNamedValue(out var method, "-m", "--method"))
        {
            method = "host";
        }

        return method switch
        {
            "host" => args.fromHost(verificationUriHandler),
            "code" => args.fromCode(verificationUriHandler),
            _ => throw new NotSupportedException("This won't happen but keeps analyzer happy :-)")
        };
    }
    
     static Task<Outcome<Grant>> fromHost(
         this string[] args,
         Action<VerificationArgs> verificationUriHandler)
     {
          // construct a desktop host, including IConfiguration, ILogging and dependency injection ...
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine("Initiates desktop host, including configuration and caching enabling 'silent' mode ...");
          Console.ResetColor();
          var grantService = args.BuildTetraPakDesktopHost(collection =>
          {
              collection
                  .AddDesktopTokenCache()        // <-- allows for silent grants (uses refresh token and/or token caching) 
                  .AddTetraPakDeviceCodeGrant(); // <-- enables the Device Code grant service
          }).ServiceCollection.BuildXpServiceProvider().GetRequiredService<IDeviceCodeGrantService>();
          var options = GrantOptions.Default();  // <-- use GrantOptions.Forced() to force a new (not cached/refreshed) grant
          return grantService.AcquireTokenAsync(options, verificationUriHandler);
     }

     static async Task<Outcome<Grant>> fromCode(
         this string[] args,
         Action<VerificationArgs> verificationUriHandler)
     {
         // please note: Unlike the above DI-approach, this code does not provide for a 'silent' device code flow
         //              To allow that a custom caching mechanism must also be provided
         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.WriteLine("SDK is consumed without a host ('silent' mode is not available) ...");
         Console.ResetColor();
         if (!args.TryGetNamedValue(out var clientId, "-cid", "--clientId"))
             return Outcome<Grant>.Fail("Please specify a client id (arg: '-cid' or '--clientId')");
         
         var secret = Environment.GetEnvironmentVariable("dc_demo_client_secret");
         var clientProvider = new TetraPakHttpClientProvider();
         var credentials = new Credentials(clientId, secret);
         var options = GrantOptions.Default() // <-- use GrantOptions.Forced() to force a new (not cached/refreshed) grant
             .WithAuthInfo(new AuthInfo(new DesktopRuntimeEnvironmentResolver()))
             .WithClientCredentials(credentials);
         var grantService = new TetraPakDeviceCodeGrantService(clientProvider);
         return await grantService.AcquireTokenAsync(options, verificationUriHandler);
     }
}

sealed class AuthInfo : IAuthInfo
{
    readonly RuntimeEnvironment _env;
    public string AuthorityUri => throw new Exception("Not used in device code flow");
    public string TokenIssuerUri => TetraPakAuthDefaults.TokenIssuerUri(_env);
    public string DeviceCodeIssuerUri => TetraPakAuthDefaults.DeviceCodeIssuerUri(_env);
    public string RedirectUri => throw new Exception("Not used in device code flow");
    public string DiscoveryDocumentUri => throw new Exception("Not used in device code flow");
    public GrantScope OidcScope => throw new Exception("Not used in device code flow");
    public bool OidcState => throw new Exception("Not used in device code flow");
    public bool OidcPkce => throw new Exception("Not used in device code flow");
    
    public AuthInfo(IRuntimeEnvironmentResolver environmentResolver)
    {
        _env = environmentResolver.ResolveRuntimeEnvironment();
    }
}