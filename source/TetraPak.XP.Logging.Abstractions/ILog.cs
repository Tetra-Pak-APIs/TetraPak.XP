using System;

namespace TetraPak.XP.Logging.Abstractions
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

        /// <summary>
        ///   Writes a new log entry.
        /// </summary>
        /// <param name="rank">
        ///   The <see cref="LogRank"/> to be applied for the message.
        /// </param>
        /// <param name="message">
        ///   (optional)<br/>
        ///   A textual message to be logged.   
        /// </param>
        /// <param name="exception">
        ///   (optional)<br/>
        ///   An <see cref="Exception"/> to be logged (usually only used when <paramref name="rank"/> = <see cref="LogRank.Error"/>).   
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A textual value used to track related events.   
        /// </param>
        /// <param name="source">
        ///   Represents the log event source (eg. class/method).
        /// </param>
        /// <param name="timestamp">
        ///   The log event time.
        /// </param>
        void Write(
            LogRank rank,
            string? message = null,
            Exception? exception = null,
            string? messageId = null,
            LogEventSource? source = null,
            DateTime? timestamp = null);

        /// <summary>
        ///   Writes a new log entry.
        /// </summary>
        /// <param name="events">
        ///   Describes the events to tbe logged.
        /// </param>
        void Write(params LogEventArgs[] events);

        /// <summary>
        ///   Gets a value indicating whether a specified <see cref="LogRank"/> is enabled.
        /// </summary>
        /// <param name="rank">
        ///   The requested <see cref="LogRank"/>
        /// </param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="LogRank"/> is supported; otherwise <c>false</c>.
        /// </returns>
        public bool IsEnabled(LogRank rank);

        /// <summary>
        ///   Constructs and returns a new <see cref="ILogSection"/>. 
        /// </summary>
        /// <param name="caption">
        ///   (optional)<br/>
        ///   A custom section caption.
        /// </param>
        /// <param name="rank">
        ///   (optional; default=<see cref="LogRank.Any"/>)<br/>
        ///   A minimum <see cref="LogRank"/> for the section
        ///   (implementor should consider defaulting this value to <see cref="LogRank.Any"/>).
        /// </param>
        /// <param name="retained">
        ///   (optional; default=true)<br/>
        ///   When set, events logged to the section will be retained until the section is disposed.
        ///   This ensures all events are presented as one block in the logged output,
        ///   rather than being distributed at the exact times they happen, which can have them spread out
        ///   and mixed with other log events, not relating to the section.
        ///   Retaining the events increases readability.
        /// </param>
        /// <param name="indent">
        ///   (optional; default=3)<br/>
        ///   A custom indentation for the section.
        /// </param>
        /// <param name="sectionSuffix">
        ///   (optional)<br/>
        ///   A custom section suffix.   
        /// </param>
        /// <returns>
        ///   A new <see cref="ILogSection"/>.
        /// </returns>
        /// <param name="source">
        ///   Represents the log event source (eg. class/method).
        /// </param>
        /// <param name="timestamp">
        ///   The log section timestamp.
        /// </param>
        ILogSection Section(
            string? caption = null,
            LogRank rank = LogRank.Any,
            bool retained = true,
            int? indent = null,
            string? sectionSuffix = null,
            LogEventSource? source = null,
            DateTime? timestamp = null);
    }
}