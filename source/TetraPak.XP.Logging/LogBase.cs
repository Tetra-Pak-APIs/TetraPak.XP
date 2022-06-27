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
        readonly object _syncRoot = new();
        readonly IConfiguration? _configuration;
        LogRank _logRank;

        /// <inheritdoc />
        public event EventHandler<LogEventArgs>? Logged;

        /// <inheritdoc />
        public LogQueryAsyncDelegate? QueryAsync { get; set; }

        /// <summary>
        ///   Gets or sets the currently enabled <see cref="LogRank"/> level.
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public LogRank Rank
        {
            get => _configuration.ResolveDefaultLogRank(_logRank);
            set => _logRank = value;
        }
        
        /// <inheritdoc />
        public virtual void Write(
            LogRank rank, 
            string? message = null, 
            Exception? exception = null,
            string? messageId = null, 
            LogEventSource? source = null,
            DateTime? timestamp = null)
        {
            Write(new LogEventArgs(source, rank, message, exception!, messageId!, timestamp));
        }
        
        /// <inheritdoc />
        public void Write(params LogEventArgs[] events)
        {
            lock (_syncRoot)
            {
                if (events.Length == 1)
                {
                    var e = events[0];
                    if (IsEnabled(e.Rank))
                    {
                        Logged?.Invoke(this, e);
                    }
                    return;
                }            
                foreach (var e in events)
                {
                    if (IsEnabled(e.Rank))
                    {
                        Logged?.Invoke(this, e);
                    }
                }
            }        
        }

        /// <inheritdoc />
        public bool IsEnabled(LogRank rank) => rank >= Rank;

        /// <inheritdoc />
        public ILogSection Section(
            string? caption = null, 
            LogRank rank = LogRank.Any, 
            bool retained = true,
            int? indent = null, 
            string? sectionSuffix = null, 
            LogEventSource? source = null,
            DateTime? timestamp = null)
        {
            return new LogSectionBase(this, caption, rank, retained, source, indent ?? 3, sectionSuffix);
        }

        public LogBase(IConfiguration? configuration)
        {
            _configuration = configuration;
            _logRank = _configuration.ResolveDefaultLogRank(LogRank.Information);
        }
        
        public LogBase(LogRank rank)
        {
            QueryAsync = null!;
            _logRank = rank;
        }
    }
}
