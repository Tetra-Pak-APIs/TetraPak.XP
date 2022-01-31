using System;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   Describes a log entry.
    /// </summary>
    public class LogEventArgs : EventArgs
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

        public LogMessageId? MessageId { get; set; }

        /// <summary>
        ///   Initializes the event arguments.
        /// </summary>
        public LogEventArgs(LogRank rank, string? message, Exception exception, LogMessageId? messageId)
        {
            Rank = rank;
            Message = message;
            Exception = exception;
            MessageId = messageId;
        }
    }
}
