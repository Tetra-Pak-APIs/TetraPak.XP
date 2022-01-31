// using System;
// using TetraPak.XP.Logging;
//
// namespace authClient.Xamarin.console
// {
//     public class ConsoleLog : ILog obsolete (moved to TetraPak.XP.Common
//     {
//         public event EventHandler<TextLogEventArgs>? Logged;
//         
//         public QueryAsyncDelegate? QueryAsync { get; set; }
//         
//         public void Write(LogRank logRank, string? message = null, Exception? exception = null)
//         {
//             string prefix;
//             switch (logRank)
//             {
//                 case LogRank.Trace:
//                     prefix = "TRC";
//                     Console.ForegroundColor = ConsoleColor.Cyan;
//                     break;
//                 
//                 case LogRank.Debug:
//                     prefix = "DBG";
//                     Console.ForegroundColor = ConsoleColor.Magenta;
//                     break;
//
//                 case LogRank.Info:
//                     prefix = "INF";
//                     Console.ForegroundColor = ConsoleColor.White;
//                     break;
//
//                 case LogRank.Warning:
//                     prefix = "WRN";
//                     Console.ForegroundColor = ConsoleColor.Yellow;
//                     break;
//
//                 case LogRank.Error:
//                     prefix = "ERR";
//                     Console.ForegroundColor = ConsoleColor.Red;
//                     break;
//
//                 default:
//                     throw new ArgumentOutOfRangeException(nameof(logRank), logRank, null);
//             }
//             Console.WriteLine($"===> [{prefix}] {message}");
//             Console.ResetColor();
//         }
//     }
// }