using System;
using authClient.viewModels;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.OIDC;
using TetraPak.XP.Logging;

namespace authClient.dependencies
{
    public static class Dependencies
    {
        static IServiceProvider s_services;

        public static IServiceProvider SetupDependencies(this App self, IServiceCollection c)
        {
            throw new NotImplementedException(); // todo we need to specify an InteractiveBrowser service for use with mobile clients 
//             var authApp = (AuthApplication)"DEV; hiJSBHQzj0v5k58j2SYTT8h54iIO8OIr; http://127.0.0.1:43423/auth";
//             c.AddTetraPakOidcAuthentication(authApp); // <-- add this
//
//             c.AddSingleton<ILog,ConsoleLog>();
//             c.AddSingleton<ILog, BasicLog>();
//
//             // view models
//             c.AddSingleton<MainViewModel>();
//             c.AddTransient<TokenVM>();
//             c.AddSingleton<AuthCodeGrantVM>();
//             c.AddTransient<TokensResultVM>();
//             c.AddTransient<ScopeTypeVM>();
//             //c.AddTransient(p => (AuthApplication) "MIG; RKAGXch5BTAGbuyj24Se88Pl0NGKylss; testping://auth")
//
//             c.AddTransient(p => authApp)
// #if DEBUG
//                 // .WithLocalAuthority(
//                 //     new Uri("http://192.168.1.62:5000/oauth2/authorize"), new Uri("http://192.168.1.62:5000/oauth2/token")
//                 //     //new Uri("https://10.69.105.9:5001/oauth2/authorize"), new Uri("https://10.69.105.9:5001/oauth2/token")
//                 //     )
// #endif
//                 ;
//             c.AddSingleton(p => self.MainPage.Navigation);
//             c.AddSingleton(p => s_services);
//
//             return s_services = c.BuildXpServiceProvider();
        }
        
        

    //     class LogFactory : ILog
    //     {
    //         public ILog Log => s_services.GetService<ILog>();
    //
    //         public event EventHandler<TextLogEventArgs> Logged;
    //         
    //         public QueryAsyncDelegate QueryAsync { get; set; }
    //         public void Write(LogRank logRank, string? message = null, Exception? exception = null)
    //         {
    //             try
    //             {
    //                 var log = s_services.GetService<ILog>();
    //                 Log.Write(logRank, message, exception);
    //             }
    //             catch (Exception nisse) // nisse
    //             {
    //                 Console.WriteLine(nisse);
    //                 throw;
    //             }
    //         }
    //
    //         public void Debug(string message) => Log.Debug(message);
    //         public void Info(string message) => Log.Info(message);
    //         public void Warning(string message) => Log.Warning(message);
    //
    //         public void Error(Exception exception, string? message = null) => Log.Error(exception, message);
    //     }
    }
}
