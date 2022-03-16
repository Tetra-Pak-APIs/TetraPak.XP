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
        const string SilentCcCommand = "scc";
        const string NewOidcTokenCommand = "ac";
        const string SilentOidcCommand = "sac";
        const string NewDcTokenCommand = "dc";
        const string SilentDcCommand = "sdc";
        const string ClearCachedGrantsCommand = "-g";
        const string ClearCachedRefreshTokensCommand = "-r";
        const string HelpCommand = "?";
        static CancellationTokenSource s_cts = new();

        public static async Task Main(string[] args)
        {
            outHelp();
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

        static void outHelp()
        {
            Console.WriteLine("+----------------------------------------------------+");
            Console.WriteLine("|  ac  = get new token using OIDC                    |");
            Console.WriteLine("|  sac = get token silently (OIDC)                   |");
            Console.WriteLine("|  cc  = get new token using Client Credentials      |");
            Console.WriteLine("|  scc = get new token silently (client credentials) |");
            Console.WriteLine("|  dc  = get new token using Device Code             |");
            Console.WriteLine("|  sdc = get new token silently (device code)        |");
            Console.WriteLine("|  c  = cancel request                               |");
            Console.WriteLine("|  -g = clear cached grants                          |");
            Console.WriteLine("|  -r = clear cached refresh tokens                  |");
            Console.WriteLine("|  ?  = print (this) help                            |");
            Console.WriteLine("|  q  = quit                                         |");
            Console.WriteLine("+----------------------------------------------------+");
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
            var cmd = command?.Trim() ?? string.Empty;
            if (isSequence(cmd, out var commandSequence))
            {
                await runSequenceAsync(commandSequence, auth);
                return;
            }
            
            Console.WriteLine($"--> {cmd}");
            switch (cmd)
            {
                case HelpCommand:
                    outHelp();
                    break;
                    
                case NewOidcTokenCommand:
                    await auth.AcquireTokenAsync(GrantType.OIDC, s_cts, false);
                    break;
                
                case SilentOidcCommand:
                    await auth.AcquireTokenAsync(GrantType.OIDC, s_cts, true);
                    break;
                
                case NewCcTokenCommand:
                    await auth.AcquireTokenAsync(GrantType.CC, s_cts, false);
                    break;
                
                case SilentCcCommand:
                    await auth.AcquireTokenAsync(GrantType.CC, s_cts, true);
                    break;
                
                case NewDcTokenCommand:
                    await auth.AcquireTokenAsync(GrantType.DC, s_cts, false);
                    break;

                case SilentDcCommand:
                    await auth.AcquireTokenAsync(GrantType.DC, s_cts, true);
                    break;

                case ClearCachedGrantsCommand:
                    await auth.ClearCachedGrantsAsync();
                    break;

                case ClearCachedRefreshTokensCommand:
                    await auth.ClearCachedRefreshTokensAsync();
                    break;

                case CancelCommand:
                    s_cts.Cancel();
                    break;
                
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }

        static async Task runSequenceAsync(string[] commands, Auth auth)
        {
            foreach (var command in commands)
            {
                await doCommandAsync(command, auth);
            }
        }

        static bool isSequence(string command, out string[] commands)
        {
            if (!command.StartsWith("#"))
            {
                commands = Array.Empty<string>();
                return false;
            }

            
            commands = command.Substring(1).Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            return true;
        }

        static string? getCommandFromConsole() => Console.ReadLine()?.ToLower();
    }
}