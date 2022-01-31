using System;

namespace TetraPak.XP.Logging
{
    class BasicLogSection : ILogSection
    {
        readonly ILog _log;
        readonly LogRank _logRank; // todo is rank really necessary for a log section? This implementation uses the original log anyway
        readonly string _indent;
        readonly int _indentLength;
        readonly string? _sectionSuffix;
        bool _isDisposed;

        public event EventHandler<LogEventArgs>? Logged;

        public LogQueryAsyncDelegate? QueryAsync { get; set; }

        public void Debug(string message) => _log.Debug(indentMessage(message));

        public void Info(string message) => _log.Information(indentMessage(message));

        public void Warning(string message) => _log.Warning(indentMessage(message));

        public void Error(Exception exception, string? message = null) => _log.Error(exception, indentMessage(message));

        public void Any(string message) => _log.Any(indentMessage(message));

        public void Write(LogRank rank, string? message, Exception? exception = null) 
            => _log.Write(rank, indentMessage(message), exception);

        string indentMessage(string? message)
        {
            if (_isDisposed) throw new InvalidOperationException("Invalid attempt to log using a disposed log section");
            return message is null ? null! : $"{_indent}{message}";
        }

        /// <inheritdoc />
        public ILogSection Section(LogRank? rank = LogRank.Any, string? caption = null, int indent = 3, string? sectionSuffix = null)
        {
            return new BasicLogSection(this, rank ?? LogRank.Any, caption, _indentLength);
        }
        
        public BasicLogSection(ILog log, LogRank logRank, string? caption, int indentLength = 3, string? sectionSuffix = null)
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