using System;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   A very basic log provider that simply dumps anything logged to the console. Intended mainly for
    ///   development/debugging purposes.
    /// </summary>
    public static class ConsoleLogHelper
    {
        static LogFormatOptions? s_options;

        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds a logger provider to output all events to the <see cref="Console"/> and then return the log instance.
        /// </summary>
        public static ILog WithConsoleLogging(this ILog log, LogFormatOptions? options = null)
        {
            log.Logged += writeToConsole;
            s_options = options ?? LogFormatOptions.Default;
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
            Console.WriteLine(args.Format(s_options));
            Console.ResetColor();
        }

    }
}