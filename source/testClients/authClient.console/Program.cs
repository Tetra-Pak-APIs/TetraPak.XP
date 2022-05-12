using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;

namespace authClient.console
{
    sealed class Program
    {
        const string QuitCommand = "q";
        const string CancelCommand = "q";
        const string CcCommand = "cc";
        const string ForcedCcCommand = "fcc";
        const string OidcCommand = "ac";
        const string ForcedOidcCommand = "fac";
        const string DcCommand = "dc";
        const string ForcedDcCommand = "fdc";
        const string TxCommand = "tx";
        const string ForcedTxCommand = "ftx";
        const string UserInformationCommand = "ui";
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
            Console.WriteLine("|  ac  = OIDC grant (silently)                       |");
            Console.WriteLine("|  fac = force new OIDC grant                        |");
            Console.WriteLine("|  cc  = Client Credentials grant (silent)           |");
            Console.WriteLine("|  fcc = force new Client Credentials grant          |");
            Console.WriteLine("|  dc  = Device Code grant                           |");
            Console.WriteLine("|  fdc = force new Device Code grant                 |");
            Console.WriteLine("|  tx  = Token Exchange grant                        |");
            Console.WriteLine("|  ftx = force new Token Exchange grant              |");
            Console.WriteLine("|  ui  = user information (using latest token)       |");
            Console.WriteLine("|  ------------------------------------------------  |");
            //Console.WriteLine("|  c  = cancel request                               |");
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

            Console.WriteLine(args.ConcatEnumerable(" "));
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
                    
                case OidcCommand:
                    await auth.AcquireTokenAsync(GrantType.OIDC, s_cts);
                    break;
                
                case ForcedOidcCommand:
                    await auth.AcquireTokenAsync(GrantType.OIDC, s_cts, true);
                    break;
                
                case CcCommand:
                    await auth.AcquireTokenAsync(GrantType.CC, s_cts);
                    break;
                
                case ForcedCcCommand:
                    await auth.AcquireTokenAsync(GrantType.CC, s_cts, true);
                    break;
                
                case DcCommand:
                    await auth.AcquireTokenAsync(GrantType.DC, s_cts);
                    break;

                case ForcedDcCommand:
                    await auth.AcquireTokenAsync(GrantType.DC, s_cts, true);
                    break;

                case TxCommand:
                    await auth.AcquireTokenAsync(GrantType.TX, s_cts);
                    break;

                case ForcedTxCommand:
                    await auth.AcquireTokenAsync(GrantType.TX, s_cts, true);
                    break;

                case UserInformationCommand:
                    await auth.GetUserInformationAsync(s_cts);
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