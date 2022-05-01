using System;

namespace TetraPak.XP.Logging.Abstractions
{
    /// <summary>
    ///   Describes a log entry.
    /// </summary>
    public sealed class LogEventArgs : EventArgs
    {
        public LogEventSource? Source { get; }

        /// <summary>
        ///   Gets the log entry rank.
        /// </summary>
        public LogRank Rank { get; internal set; }

        /// <summary>
        ///   Gets the log entry message (if any).
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        ///   Gets a log entry error (if any).
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        ///   A unique string value for tracking related events.
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        ///   The date/time when the log event occured.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///   Initializes the event arguments.
        /// </summary>
        public LogEventArgs(
            LogEventSource? source,
            LogRank rank,
            string? message,
            Exception? exception,
            string? messageId,
            DateTime? timestamp)
        {
            Source = source;
            Rank = rank;
            Message = message;
            Exception = exception;
            MessageId = messageId;
            Timestamp = timestamp ?? XpDateTime.UtcNow;
        }
    }

    public static class LogEventArgsHelper
    {
        /// <summary>
        ///   (fluent api)<br/>
        ///   Assigns the <see cref="LogEventArgs.Rank"/> property and returns <c>this</c>.
        /// </summary>
        public static LogEventArgs WithRank(this LogEventArgs args, LogRank rank)
        {
            args.Rank = rank;
            return args;
        }
    }

    public sealed class LogFormatOptions
    {
        public bool OmitTimestamp { get; set; }

        public bool OmitSource { get; set; }

        public bool OmitRank { get; set; }

        public bool OmitPrefix { get; set; }

        public bool OmitMessageId { get; set; }

        public static LogFormatOptions Default => new()
        {
            OmitTimestamp = false,
            OmitSource = false,
            OmitRank = false,
            OmitPrefix = false,
            OmitMessageId = false
        };
    }

    public static class LogFormatOptionsHelper
    {
        public static LogFormatOptions WithOmitTimestamp(this LogFormatOptions options, bool value)
        {
            options.OmitTimestamp = value;
            return options;
        }
    }
}