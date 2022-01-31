using System;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   Provides a basic logging mechanism to the package.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        ///   Triggered whenever a log entry gets added.
        /// </summary>
        event EventHandler<LogEventArgs>? Logged;

        /// <summary>
        ///   Gets or sets a delegate used for querying the log.
        /// </summary>
        LogQueryAsyncDelegate? QueryAsync { get; set; }

        void Write(LogRank rank, string? message = null, Exception? exception = null, LogMessageId? messageId = null);

        public bool IsEnabled(LogRank rank);
        
        ILogSection Section(LogRank? rank = null, string? caption = null, int indent = 3, string? sectionSuffix = null);
    }
    
    public interface ILogSection : ILog, IDisposable
    {
    }
}
