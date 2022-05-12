using System;
using System.Collections.Generic;
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
        readonly LogRank _rank;
        readonly string _indent;
        readonly int _indentLength;
        readonly string? _sectionSuffix;
        bool _isDisposed;
        readonly List<LogEventArgs>? _retainedEvents;
        readonly LogRank _captionRank;

        public event EventHandler<LogEventArgs>? Logged;

        public LogQueryAsyncDelegate? QueryAsync { get; set; }

        public bool IsRetained => _retainedEvents is { }; 

        string indentMessage(string? message, int indentAdjust = 0)
        {
            if (_isDisposed) throw new InvalidOperationException("Invalid attempt to log using a disposed log section");
            var indent = indentAdjust == 0 ? _indent : new string(' ', Math.Max(0, _indentLength + indentAdjust));
            return message is null ? null! : $"{indent}{message}";
        }

        /// <inheritdoc />
        public void Write(
            LogRank rank, 
            string? message = null, 
            Exception? exception = null, 
            string? messageId = null, 
            LogEventSource? source = null,
            DateTime? timestamp = null)
        {
            doWrite(rank, message, exception, messageId, source);
        }

        /// <inheritdoc />
        public void Write(params LogEventArgs[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                var a = events[i];
                Write(a.Rank, a.Message, a.Exception, a.MessageId);
            }
        }

        void doWrite(LogRank rank, string? message, Exception? exception, string? messageId, LogEventSource? source)
        {
            if (IsRetained)
            {
                _retainedEvents!.Add(
                    new LogEventArgs(source,
                        rank, 
                        indentMessage(message), 
                        exception, 
                        messageId, 
                        XpDateTime.Now));
                return;
            }
            _log.Write(rank, indentMessage(message), exception, messageId, source);
        }

        void doWriteCaption(string? caption, LogEventSource? source)
        {
            if (!IsRetained)
            {
                _log.Write(_captionRank, indentMessage(caption, -_indentLength), source:source);
                return;
            }

            _retainedEvents!.Add(new LogEventArgs(source, _captionRank, indentMessage(caption, -_indentLength), null, null, XpDateTime.Now));
        }

        /// <inheritdoc />
        public bool IsEnabled(LogRank rank)
        {
            return _log.IsEnabled(rank);
        }

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
            return new LogSectionBase(
                this, 
                caption, 
                rank, 
                retained,
                source, 
                _indentLength + indent ?? _indentLength, 
                sectionSuffix);
        }
        
        public LogSectionBase(
            ILog log, 
            string? caption,
            LogRank rank,
            bool isRetained,
            LogEventSource? source = null, 
            int indentLength = 3,
            string? sectionSuffix = null)
        {
            _log = log;
            _captionRank = rank;
            _rank = rank == LogRank.Any ? TypeHelper.GetDefaultValue<LogRank>() : rank;
            _indentLength = indentLength;
            _indent = new string(' ', indentLength);
            _sectionSuffix = sectionSuffix;
            _retainedEvents = isRetained ? new List<LogEventArgs>() : null;
            if (!string.IsNullOrEmpty(caption))
            {
                doWriteCaption(indentMessage(caption, -_indentLength), source: source);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _isDisposed) 
                return;

            if (!IsRetained)
            {
                _log.Write(_rank, _sectionSuffix);
                _isDisposed = true;
                return;
            }
            doWriteCaption(_sectionSuffix, null);
            _log.Write(_retainedEvents!.ToArray());
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}