using System;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   A basic <see cref="ILogSection"/> implementation.
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    class LogSectionBase : ILogSection
    {
        readonly ILog _log;
        readonly LogRank _logRank;
        readonly string _indent;
        readonly int _indentLength;
        readonly string? _sectionSuffix;
        bool _isDisposed;

        public event EventHandler<LogEventArgs>? Logged;

        public LogQueryAsyncDelegate? QueryAsync { get; set; }

        string indentMessage(string? message)
        {
            if (_isDisposed) throw new InvalidOperationException("Invalid attempt to log using a disposed log section");
            return message is null ? null! : $"{_indent}{message}";
        }

        public void Write(LogRank rank, string? message = null, Exception? exception = null, string? messageId = null)
        {
            _log.Write(rank, indentMessage(message), exception, messageId);
        }

        public bool IsEnabled(LogRank rank)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ILogSection Section(LogRank? rank = LogRank.Any, string? caption = null, int indent = 3, string? sectionSuffix = null)
        {
            return new LogSectionBase(this, rank ?? LogRank.Any, caption, _indentLength);
        }
        
        public LogSectionBase(ILog log, LogRank logRank, string? caption, int indentLength = 3, string? sectionSuffix = null)
        {
            _log = log;
            _logRank = logRank;
            _indentLength = indentLength;
            _indent = new string(' ', indentLength);
            _sectionSuffix = sectionSuffix;
            if (!string.IsNullOrEmpty(caption))
            {
                log.Write(logRank, caption);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _isDisposed) 
                return;
            
            _log.Write(_logRank, _sectionSuffix);
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}