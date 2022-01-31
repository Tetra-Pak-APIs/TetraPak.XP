using System;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   Provides a very basic <see cref="ILog"/> implementation.
    /// </summary>
    /// <remarks>
    ///   Invoking the different log methods of this implementation simply
    ///   triggers the <see cref="Logged"/> event. This can be utilized to
    ///   dispatch the actual log entries to various logging solutions.
    /// </remarks>
    public class BasicLog : ILog
    {
        /// <inheritdoc />
        public event EventHandler<LogEventArgs>? Logged;

        /// <inheritdoc />
        public LogQueryAsyncDelegate? QueryAsync { get; set; }

        /// <summary>
        ///   Gets or sets the currently enabled <see cref="LogRank"/> level.
        /// </summary>
        public LogRank Rank { get; set; } = LogRank.Trace;

        /// <inheritdoc />
        public virtual void Write(LogRank rank, string? message = null, Exception? exception = null, LogMessageId? messageId = null)
        {
            if (IsEnabled(rank))
            {
                Logged?.Invoke(this, new LogEventArgs(rank, message, exception!, messageId!));
            }
        }

        /// <inheritdoc />
        public bool IsEnabled(LogRank rank) => rank >= Rank;

        public ILogSection Section(LogRank? rank = null, string? caption = null, int indent = 3, string? sectionSuffix = null)
        {
            return new BasicLogSection(this, rank ?? LogRank.Any, caption);
        }

        public BasicLog() => QueryAsync = null!;
    }
}
