using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;
using TetraPak.XP.OAuth2.DeviceCode;

// construct a desktop host also provides access to .NET's IConfiguration code api ... 
var info = args.BuildTetraPakDesktopHost(collection =>
{
    collection
        .AddDesktopTokenCache()        // <-- allows for silent grants (uses refresh token and/or token caching) 
        .AddTetraPakDeviceCodeGrant(); // <-- enables the Device Code grant service
});

// get the service locator ...
var services = info.ServiceServiceCollection.BuildXpServiceProvider();
var logger = services.GetService<ILogger<GrantOptions>>();

// run the Device Code grant request ...
Console.WriteLine("Press any key to get Device Code grant:");
Console.ReadKey();
var dcGrantService = services.GetRequiredService<IDeviceCodeGrantService>();
var outcome = await dcGrantService.AcquireTokenAsync(GrantOptions.Default(), // <-- use GrantOptions.Forced() to guarantee a new (not cached/refreshed) grant
    e =>
    {
        Console.WriteLine($"Please very code '{e.UserCode}' on: {e.VerificationUri} ...");
        
    });

// output the outcome
if (!outcome)
{
    Console.WriteLine("FAIL!");
    Console.WriteLine(outcome.Message);
    return;
}

Console.WriteLine("SUCCESS!");
var grant = outcome.Value!;
Console.WriteLine("Token(s):");
foreach (var token in grant.Tokens!)
{
    Console.WriteLine($"  {token.Role}={token.Token}");
}
Console.WriteLine($"Scope={grant.Scope}");