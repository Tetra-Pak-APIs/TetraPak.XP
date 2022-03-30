using System;

namespace TetraPak.XP.Logging.Abstractions;

/// <summary>
///   Represents a single log event.
/// </summary>
public interface ILogEvent
{
    LogRank Rank { get; }

    DateTime Time { get; }

    string Message { get; }
}
