using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;

namespace authClient.console
{
    class Program
    {
        const string QuitCommand = "q";
        const string CancelCommand = "c";
        const string NewCcTokenCommand = "cc";
        const string NewOidcTokenCommand = "ac";
        const string NewDcTokenCommand = "dc";
        const string SilentTokenCommand = "sl";
        static CancellationTokenSource s_cts = new();
        // static IServiceProvider s_serviceProvider;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("+---------------------------------------------------+");
            Console.WriteLine("|  ac = get new token using OIDC                    |");
            Console.WriteLine("|  sl = get token silently (OIDC)                   |");
            Console.WriteLine("|  cc = get new token using Client Credentials      |");
            Console.WriteLine("|  dc = get new token using Device Code             |");
            Console.WriteLine("|  c  = cancel request                              |");
            Console.WriteLine("|  q  = quit                                        |");
            Console.WriteLine("+---------------------------------------------------+");

            var auth = new Auth(args);
            prompt();
            var command = getCommandFrom(args) ?? getCommandFromConsole();
            while (command != QuitCommand)
            {
                await doCommandAsync(command, auth);
                prompt();
                command = getCommandFromConsole();
                if (s_cts.IsCancellationRequested)
                {
                    s_cts = new CancellationTokenSource();
                }
            }
        }

        static void prompt() => Console.Write("Please specify action:");

        static string? getCommandFrom(IReadOnlyList<string> args)
        {
            if (args.Count == 0)
                return null;

            Console.WriteLine(args.ConcatCollection(" "));
            var cmd = args[0];
            return cmd.Length > 1 ? cmd.ToLower() : null;
        }

        static async Task doCommandAsync(string? command, Auth auth)
        {
            switch (command)
            {
                case NewOidcTokenCommand:
                    auth.NewTokenAsync(GrantType.OIDC, s_cts);
                    break;
                
                case NewCcTokenCommand:
                    auth.NewTokenAsync(GrantType.CC, s_cts);
                    break;
                
                case NewDcTokenCommand:
                    auth.NewTokenAsync(GrantType.DC, s_cts);
                    break;
                
                case SilentTokenCommand:
                    await auth.SilentTokenAsync();
                    break;
                
                case CancelCommand:
                    s_cts.Cancel();
                    break;
                
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }

        static string? getCommandFromConsole() => Console.ReadLine()?.ToLower();
    }
}