#Demo: Device Code in Console app 

This is a .NET 6 console app to demonstrate how to easily enable and consume the Device Code grant service from Tetra Pak. 

There are two aspects you need to consider, as demonstrated by this app:

1. Configuration (local and environment)
2. The code (in Program.cs)

## Configuration

To consume the Device Code grant service you need a registered "app" (a.k.a. an "app registration"). This allows the Tetra Pak API management system to identify your app and resolve its authorization. Every app registration is identified by its "client id" (a.k.a. "consumer key"). The app registration also contains a "client secret" (a.k.a. "consumer secret"). Some grant types, such as "authorization code", only needs the client id but the Device Code grant also requires your client passes the client secret as well.

The client id value is not considered sensitive. In some grant types this value is even sent as part of the (unencrypted) URL. Not so with the client secret however. That value should never be shared outside you local environment and you should take steps to prevent this. For this reason this demonstration console app is setup to read the client secret from an environment variable ("tx_demo_client_secret"). The client secret is then expressed as "`$(Env/tx_demo_client_secret)`" in the appsettings.json file. 

This approach to store a client secret may be sufficient but please note that there are better ways. By not storing the client secret in the appsettings.json file you have ensured there is no risk the secret is mistakenly distributed. You can even commit the appsettings.json file to your version control system, such as GitHub or Azure DevOps. 

So, to be able to run this demo app you need to create an environment variable and set its value to the client secret associated with the client id you specify in the appsettings.json file. You can use a different environment variable identifier but please ensure this is also reflected in the appsettings.json file: `$(Env/<name of your environment variable>)`.

##The Code
With the configuration out of the way let's look at the code in Program.cs:

The `BuildTetraPakDesktopHost` extension method is an efficient and fast way to quickly build and run a host, including support for the .NET `IConfiguration` framework (which allows for local and environment configuration). 

```c#
var info = args.BuildTetraPakDesktopHost(collection =>
{
    collection
        .AddDesktopTokenCache()        // <-- allows for silent grants (uses refresh token and/or token caching) 
        .AddTetraPakDeviceCodeGrant(); // <-- enables the Device Code grant service
});

// get the service locator ...
var services = info.ServiceServiceCollection.BuildXpServiceProvider();
```
You can call the `BuildTetraPakDesktopHost` extension method without any arguments but that would not enable any services except the base dependency injection API (`IServiceCollection` and `IServiceProvider`) and the configuration framework (`IConfiguration` service API).

This example also introduces the ability to cache tokens and, of course, the Device Code grant service to be demonstrated.

With the host and DI services configured the code builds the service locator.

The demo app then awaits the user's input before instantiating the Device Code grant service and invokes it to acquire a Device Code grant:

```c#
var dcGrantService = services.GetRequiredService<IDeviceCodeGrantService>();
var outcome = await dcGrantService.AcquireTokenAsync(GrantOptions.Default(), // <-- use GrantOptions.Forced() to guarantee a new (not cached/refreshed) grant
    e =>
    {
        Console.WriteLine($"Please very code '{e.UserCode}' on: {e.VerificationUri} ...");
        
    });
```

This is the core of this demonstration. The Device Code grant service's `AcquireTokenAsync` method is invoked with some options and a callback handler. The `GrantOptions` is easily created by calling the type's static `Default` method. We won't delve into the details here but the important part is this allows the Device Code grant service to acquire the grant "silently". What this means is it will look for an existing, cached, grant and reuse it if it is still valid.  If this fails it will then look for a cached refresh token, acquired in an earlier grant request. If one is found it will automatically invoke the SDK's Refresh Token grant service (implicitly set up by the `IServiceCollection.AddTetraPakDeviceCodeGrant` extension method) to acquire a new grant, with no need for user interaction.

Only when no cached token or refresh token was found or yielded a new grant will the Device Code grant service initiate a new (full) Device Code grant request. As part of such a request the Device Code grant service will call back with the device code to be verified by the user and the URL to be used for this verification. The demo app simply presents this device code and URL in the console. The user can simply copy the device code and then browse to the specified URL in a browser where the device code and be submitted.

The outcome of this request is reflected by an `Outcome<Grant>` value. The `Outcome<T>` class is heavily used throughout the SDK as a simple means to reflect any type of outcome in asynchronous operations. The class is implicitly type compatible `bool` and can therefore be used in control statements while supporting the clear code approach to coding.

The demo app then proceeds to either present a successful outcome, along with the tokens provided by the resulting `Grant` or a message to reflect why the device code grant request failed.