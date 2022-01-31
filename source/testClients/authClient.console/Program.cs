using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TetraPak.XP.Logging;

namespace authClient.console
{
    class Program
    {
        static readonly ILog s_log = new BasicLog().WithConsoleLogging();

        const char QuitCommand = 'q';
        const char NewTokenCommand = 'n';
        const char SilentTokenCommand = 's';

        public static async Task Main(string[] args)
        {
            Console.WriteLine("+---------------------------------------------------+");
            Console.WriteLine("|  n = get new token                                |");
            Console.WriteLine("|  s = get token silently                           |");
            Console.WriteLine("|  q = quit                                         |");
            Console.WriteLine("+---------------------------------------------------+");

            var auth = new Auth(s_log);
            var command = getCommandFrom(args) ?? getCommandFromConsole();
            while (command != QuitCommand)
            {
                await doCommandAsync(command, auth);
                command = getCommandFromConsole();
            }
        }

        static char? getCommandFrom(IReadOnlyList<string> args)
        {
            if (args.Count == 0)
                return null;

            var cmd = args[0];
            return cmd.Length > 1 ? null : cmd[0];
        }

        static async Task doCommandAsync(char command, Auth auth)
        {
            switch (command)
            {
                case NewTokenCommand:
                    await auth.NewTokenAsync();
                    break;
                
                case SilentTokenCommand:
                    await auth.SilentTokenAsync();
                    break;
            }
        }

        static char getCommandFromConsole() => char.ToLower(Console.ReadKey().KeyChar);
    }
}