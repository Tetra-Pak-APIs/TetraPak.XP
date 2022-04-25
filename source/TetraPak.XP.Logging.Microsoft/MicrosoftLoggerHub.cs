using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Logging.Microsoft;

sealed class MicrosoftLoggerHub : LogBase, ILogger
{
    readonly ILogger _logger;
    readonly ILog? _log;
    readonly LogFormatOptions? _formatOptions;

    public void Log<TState>(
        LogLevel logLevel, 
        EventId eventId, 
        TState state, 
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var args = new LogEventArgs(null, logLevel.ToLogRank(), formatter(state, exception), exception, eventId.ToString(), null);
        writeToBoth(args);
    }

    public override void Write(
        LogRank rank, 
        string? message = null, 
        Exception? exception = null, 
        string? messageId = null,
        LogEventSource? source = null,
        DateTime? timestamp = null)
    {
        var args = new LogEventArgs(source, rank, message, exception, messageId, timestamp);
        writeToBoth(args);
    }

    void writeToBoth(LogEventArgs args)
    {
        _log?.Write(args);
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        _logger.Log(args.Rank.ToLogLevel(Rank), args.Format(_formatOptions));
    }

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
    
    public MicrosoftLoggerHub(ILogger logger, ILog? log, IConfiguration? configuration, LogFormatOptions? formatOptions) 
    : base(configuration)
    {
        _logger = logger;
        _log = log;
        _formatOptions = formatOptions;
    }
}