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
        const string SilentCcCommand = "ccs";
        const string NewOidcTokenCommand = "ac";
        const string SilentOidcCommand = "acs";
        const string NewDcTokenCommand = "dc";
        const string SilentDcCommand = "dcs";
        static CancellationTokenSource s_cts = new();

        public static async Task Main(string[] args)
        {
            Console.WriteLine("+----------------------------------------------------+");
            Console.WriteLine("|  ac  = get new token using OIDC                    |");
            Console.WriteLine("|  acs = get token silently (OIDC)                   |");
            Console.WriteLine("|  cc  = get new token using Client Credentials      |");
            Console.WriteLine("|  ccs = get new token silently (client credentials) |");
            Console.WriteLine("|  dc  = get new token using Device Code             |");
            Console.WriteLine("|  dcs = get new token silently (device code)        |");
            Console.WriteLine("|  c  = cancel request                               |");
            Console.WriteLine("|  q  = quit                                         |");
            Console.WriteLine("+----------------------------------------------------+");

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

        static Task doCommandAsync(string? command, Auth auth)
        {
            switch (command)
            {
                case NewOidcTokenCommand:
                    auth.AcquireTokenAsync(GrantType.OIDC, s_cts, false);
                    break;
                
                case SilentOidcCommand:
                    auth.AcquireTokenAsync(GrantType.OIDC, s_cts, true);
                    break;

                case NewCcTokenCommand:
                    auth.AcquireTokenAsync(GrantType.CC, s_cts, false);
                    break;
                
                case SilentCcCommand:
                    auth.AcquireTokenAsync(GrantType.CC, s_cts, true);
                    break;
                
                case NewDcTokenCommand:
                    auth.AcquireTokenAsync(GrantType.DC, s_cts, false);
                    break;

                case SilentDcCommand:
                    auth.AcquireTokenAsync(GrantType.DC, s_cts, true);
                    break;

                case CancelCommand:
                    s_cts.Cancel();
                    break;
                
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }

            return Task.CompletedTask;
        }

        static string? getCommandFromConsole() => Console.ReadLine()?.ToLower();
    }
}