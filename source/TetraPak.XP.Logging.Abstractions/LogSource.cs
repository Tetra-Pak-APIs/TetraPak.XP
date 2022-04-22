using System.Runtime.CompilerServices;

namespace TetraPak.XP.Logging.Abstractions;

/// <summary>
///   Represents the source of a log event (such as a method).
/// </summary>
public sealed class LogSource
{
    readonly string _stringValue;

    public override string ToString() => _stringValue;

    bool Equals(LogSource other)
    {
        return _stringValue == other._stringValue;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((LogSource)obj);
    }

    public override int GetHashCode()
    {
        return _stringValue.GetHashCode();
    }

    public static bool operator ==(LogSource? left, LogSource? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(LogSource? left, LogSource? right)
    {
        return !Equals(left, right);
    }

    public LogSource([CallerMemberName] string? caller = null, [CallerFilePath] string? callerFile = null, [CallerLineNumber] int callerLine = 0)
    {
        _stringValue = $"{caller}@{callerFile} (#{callerLine})";
    }
}