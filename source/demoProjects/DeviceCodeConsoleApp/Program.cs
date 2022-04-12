using System;
using DeviceCodeConsoleApp;

var authOutcome = await args.AcquireDeviceCodeTokenAsync(e =>
{
    Console.WriteLine($"Please very code '{e.UserCode}' on: {e.VerificationUri} ...");
    
});

if (!authOutcome)
{
    Console.WriteLine("FAIL!");
    Console.WriteLine(authOutcome.Message);
    return;
}

Console.WriteLine("SUCCESS!");
var grant = authOutcome.Value!;
Console.WriteLine("Token(s):");
foreach (var token in grant.Tokens!)
{
    Console.WriteLine($"  {token.Role}={token.Token}");
}
Console.WriteLine($"Scope={grant.Scope}");