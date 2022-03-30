using System;

namespace TetraPak.XP.Logging.Abstractions
{
    /// <summary>
    ///   Describes a log entry.
    /// </summary>
    public sealed class LogEventArgs : EventArgs
    {
        /// <summary>
        ///   Gets the log entry rank.
        /// </summary>
        public LogRank Rank { get; }

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
        ///   Initializes the event arguments.
        /// </summary>
        public LogEventArgs(LogRank rank, string? message, Exception exception, string? messageId)
        {
            Rank = rank;
            Message = message;
            Exception = exception;
            MessageId = messageId;
        }
    }
}
