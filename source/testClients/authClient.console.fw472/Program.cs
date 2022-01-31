﻿using System;
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
            var command = getCommand();
            while (command != QuitCommand)
            {
                await doCommandAsync(command, auth);
                command = getCommand();
            }
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

        static char getCommand() => char.ToLower(Console.ReadKey().KeyChar);
    }
}