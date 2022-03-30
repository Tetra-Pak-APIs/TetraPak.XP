using System;
using System.Collections.Generic;
using System.Text;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   Provides convenient helper methods for logging.
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        ///   Gets (or sets) a default <see cref="string"/> value to be inserted as a prefix to all
        ///   log entries. This can be used to distinguish entries sourced by your code from other log entries..  
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public static string Prefix { get; set; } = "------>";
        
        /// <summary>
        ///   Writes a trace log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="message">
        ///   A message to be written to <paramref name="log"/>.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        /// <seealso cref="System.Diagnostics.Trace"/>
        public static void Trace(this ILog? log, string message, string? messageId = null) 
            => log?.Trace(() => message, messageId);

        /// <summary>
        ///   Writes a trace log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="messageHandler">
        ///   A message handler (only invoked when <see cref="System.Diagnostics.Trace"/> is enabled).
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Trace(this ILog? log, Func<string> messageHandler, string? messageId = null)
        {
            if (log is null || !log.IsEnabled(LogRank.Trace))
                return;
            
            log.Write(LogRank.Trace, messageHandler(), null!, messageId);
        }
        
        /// <summary>
        ///   Writes a debug log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="message">
        ///   A message to be written to <paramref name="log"/>.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Debug(this ILog? log, string message, string? messageId = null) 
            => log?.Debug(() => message, messageId);

        /// <summary>
        ///   Writes a debug log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="messageHandler">
        ///   A message handler (only invoked when <see cref="LogRank.Debug"/> is enabled).
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Debug(this ILog? log, Func<string> messageHandler, string? messageId = null)
        {
            if (log is null || !log.IsEnabled(LogRank.Debug))
                return;
            
            log.Write(LogRank.Debug, messageHandler(), null!, messageId);
        }
        
        /// <summary>
        ///   Writes a warning log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="message">
        ///   A message to be written to <paramref name="log"/>.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking an event through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Warning(this ILog? log, string message, string? messageId = null) 
            => log.Warning(() => message, messageId);

        /// <summary>
        ///   Writes a warning log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="messageHandler">
        ///   A message handler (only invoked when <see cref="LogRank.Warning"/> is enabled).
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Warning(this ILog? log, Func<string> messageHandler, string? messageId = null)
        {
            if (log is null || !log.IsEnabled(LogRank.Warning))
                return;
            
            log.Write(LogRank.Warning, messageHandler(), null!, messageId);
        }

        /// <summary>
        ///   Writes an information log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="message">
        ///   A message to be written to <paramref name="log"/>.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking an event through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Information(this ILog? log, string message, string? messageId = null) 
            => log.Information(() => message, messageId);

        /// <summary>
        ///   Writes an information log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="messageHandler">
        ///   A message handler (only invoked when <see cref="LogRank.Information"/> is enabled).
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking an event through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Information(this ILog? log, Func<string> messageHandler, string? messageId = null)
        {
            if (log is null || !log.IsEnabled(LogRank.Information))
                return;
            
            log.Write(LogRank.Information, messageHandler(), null!, messageId);
        }
        
        /// <summary>
        ///   Writes an <see cref="Exception"/> (error) log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="exception">
        ///   An <see cref="Exception"/> to be logged.
        /// </param>
        /// <param name="message">
        ///   A message to be written to <paramref name="log"/>.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Error(this ILog? log, Exception exception, string? message = null, string? messageId = null) 
            => log.Error(exception, () => message, messageId);

        /// <summary>
        ///   Writes an information log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="exception">
        ///   An <see cref="Exception"/> to be logged.
        /// </param>
        /// <param name="messageHandler">
        ///   A message handler (only invoked when <see cref="LogRank.Error"/> is enabled).
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Error(this ILog? log, Exception exception, Func<string?> messageHandler, string? messageId = null)
        {
            if (log is null || !log.IsEnabled(LogRank.Error))
                return;
            
            log.Write(LogRank.Error, messageHandler(), exception, messageId);
        }

        /// <summary>
        ///   Adds a log message for any log rank (such as a log section entry/exit.
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="message">
        ///   The message to be logged.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Any(this ILog? log, string message, string? messageId = null) 
            => log.Any(() => message, messageId);
        
        /// <summary>
        ///   Writes an information log entry in a standardized format.  
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="messageHandler">
        ///   A message handler (only invoked when <see cref="LogRank.Any"/> is enabled).
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        public static void Any(this ILog? log, Func<string> messageHandler, string? messageId = null)
        {
            if (log is null || !log.IsEnabled(LogRank.Information))
                return;
            
            log.Write(LogRank.Any, messageHandler(), null!, messageId);
        }

        /// <summary>
        ///   Create a standardized logging format and returns the result.
        /// </summary>
        /// <param name="rank">
        ///   The message <see cref="LogRank"/> level.
        /// </param>
        /// <param name="message">
        ///   The message to be logged.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        /// <returns>
        ///   A standardized logging message (<see cref="string"/> value).
        /// </returns>
        public static string Format(LogRank rank, string message, LogMessageId? messageId = null)
        {
            return messageId is null
                ? $"{Prefix} [{rank.ToAbbreviatedString()}] {message}"
                : $"{Prefix} <{messageId}> [{rank.ToAbbreviatedString()}] {message}";
        }

        /// <summary>
        ///   Creates a standardized logging format and returns the result.
        /// </summary>
        /// <param name="rank">
        ///   The message <see cref="LogRank"/> level.
        /// </param>
        /// <param name="exception">
        ///   An <see cref="Exception"/> to be logged.
        /// </param>
        /// <param name="message">
        ///   A message to be logged.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        /// <returns>
        ///   A standardized logging message (<see cref="string"/> value).
        /// </returns>
        public static string Format(LogRank rank, Exception? exception, string? message, LogMessageId? messageId = null)
        {
            message ??= exception?.Message ?? "(NO MESSAGE)";
            var formatted = messageId is null
                ? $"{Prefix} [{rank.ToAbbreviatedString()}] {message}"
                : $"{Prefix} <{messageId}> [{rank.ToAbbreviatedString()}] {message}";

            var sb = new StringBuilder(formatted);
            if (rank > LogRank.Debug || exception is null)
                return sb.ToString();

            sb.AppendLine(exception.ToString());
            if (exception.InnerException is { })
            {
                sb.AppendLine(exception.InnerException.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        ///   Creates a standardized logging format from the <see cref="LogEventArgs"/> and returns the result.
        /// </summary>
        /// <returns>
        ///   A standardized logging message (<see cref="string"/> value).
        /// </returns>
        public static string Format(this LogEventArgs args) => Format(args.Rank, args.Exception, args.Message, args.MessageId);

        /// <summary>
        ///   Writes the contents of a <see cref="IDictionary{TKey,TValue}"/> to a <see cref="ILog"/>
        ///   for a specified <see cref="LogRank"/>.
        /// </summary>
        /// <param name="log">
        ///   The logging provider.
        /// </param>
        /// <param name="dictionary">
        ///   The dictionary to be written.
        /// </param>
        /// <param name="rank">
        ///   The <see cref="LogRank"/> to be used for teh log entry.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking related events through the log (mainly for diagnostics purposes).
        /// </param>
        /// <typeparam name="TKey">
        ///   The dictionary key <see cref="Type"/>.
        /// </typeparam>
        /// <typeparam name="TValue">
        ///   The dictionary value <see cref="Type"/>.
        /// </typeparam>
        public static void LogDictionary<TKey,TValue>(
            this ILog log, 
            IDictionary<TKey,TValue> dictionary,
            LogRank rank,
            string? messageId = null) 
        where TKey : notnull
        {
            var message = new StringBuilder();
            foreach (var pair in dictionary)
            {
                var key = pair.Key;
                var value = pair.Value;
                message.AppendLine($"{key.ToString()}={value?.ToString()}");
            }
            log.Write(rank, message.ToString(), messageId: messageId);
        }
    }
}