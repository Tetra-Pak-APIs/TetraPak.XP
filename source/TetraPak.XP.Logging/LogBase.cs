using System;
using Microsoft.Extensions.Configuration;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   Provides a basic <see cref="ILog"/> implementation.
    /// </summary>
    /// <remarks>
    ///   Invoking the different log methods of this implementation simply
    ///   triggers the <see cref="Logged"/> event. This can be utilized to
    ///   dispatch the actual log entries to various logging solutions.
    /// </remarks>
    public class LogBase : ILog
    {
        /// <inheritdoc />
        public event EventHandler<LogEventArgs>? Logged;

        /// <inheritdoc />
        public LogQueryAsyncDelegate? QueryAsync { get; set; }

        /// <summary>
        ///   Gets or sets the currently enabled <see cref="LogRank"/> level.
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public LogRank Rank { get; set; }

        /// <inheritdoc />
        public virtual void Write(LogRank rank, string? message = null, Exception? exception = null, string? messageId = null)
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
            return new LogSectionBase(this, rank ?? LogRank.Any, caption);
        }

        public LogBase(IConfiguration? configuration)
        {
            QueryAsync = null!;
            Rank = configuration.ResolveDefaultLogRank(LogRank.Information);
        }
    }
}
