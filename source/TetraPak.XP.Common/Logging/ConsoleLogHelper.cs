using System;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   A very basic log provider that simply dumps anything logged to the console. Intended mainly for
    ///   development/debugging purposes.
    /// </summary>
    public static class ConsoleLogHelper
    {
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds a logger provider to write all events to the <see cref="Console"/> and then return the log instance.
        /// </summary>
        public static ILog WithConsoleLogging(this ILog log)
        {
            log.Logged += writeToConsole!;
            return log;
        }

        static void writeToConsole(object sender, LogEventArgs args)
        {
            Console.ForegroundColor = args.Rank switch
            {
                LogRank.Trace => ConsoleColor.Cyan,
                LogRank.Debug => ConsoleColor.Magenta,
                LogRank.Information => ConsoleColor.White,
                LogRank.Warning => ConsoleColor.Yellow,
                LogRank.Error => ConsoleColor.Red,
                LogRank.Any => ConsoleColor.DarkYellow,
                LogRank.None => throw new ArgumentOutOfRangeException(nameof(args.Rank), args.Rank, null),
                _ => throw new ArgumentOutOfRangeException(nameof(args.Rank), args.Rank, null)
            };
            Console.WriteLine(args.Format());
            Console.ResetColor();
        }

    }
}