using System;

namespace TetraPak.XP.Logging.Abstractions;

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
    void Write(LogRank rank, string? message = null, Exception? exception = null, string? messageId = null, LogSource? source = null);
    
    /// <summary>
    ///   Writes a new log entry.
    /// </summary>
    /// <param name="args">
    ///   Describes the event to tbe logged.
    /// </param>
    void Write(LogEventArgs args);

    /// <summary>
    ///   Gets a value indicating whether a specified <see cref="LogRank"/> is enabled.
    /// </summary>
    /// <param name="rank">
    ///   The requested <see cref="LogRank"/>
    /// </param>
    /// <returns></returns>
    public bool IsEnabled(LogRank rank);

    /// <summary>
    ///   Constructs and returns a new <see cref="ILogSection"/>. 
    /// </summary>
    /// <param name="rank">
    ///   (optional)<br/>
    ///   A minimum <see cref="LogRank"/> for the section
    ///   (implementor should consider defaulting this value to <see cref="LogRank.Any"/>).
    /// </param>
    /// <param name="caption">
    ///   (optional)<br/>
    ///   A custom section caption.
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
    ILogSection Section(LogRank? rank = null, string? caption = null, int indent = 3, string? sectionSuffix = null, LogSource? source = null);
}
