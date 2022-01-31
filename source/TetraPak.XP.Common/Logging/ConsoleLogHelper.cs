using System;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   A very basic log provider that simply dumps anything logged to the console. Intended mainly for
    ///   development/debugging purposes.
    /// </summary>
    public static class ConsoleLogHelper
    {
        public static ILog WithConsoleLog(this ILog log)
        {
            log.Logged += writeToConsole;
            return log;
        }

        static void writeToConsole(object sender, LogEventArgs args)
        {
            switch (args.Rank)
            {
                case LogRank.Trace:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                
                case LogRank.Debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;

                case LogRank.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case LogRank.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogRank.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(args.Rank), args.Rank, null);
            }
            Console.WriteLine(args.Format());
            Console.ResetColor();
        }

    }
}