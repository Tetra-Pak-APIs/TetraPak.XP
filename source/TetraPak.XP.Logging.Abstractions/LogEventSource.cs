using System.Runtime.CompilerServices;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Logging.Abstractions
{

    /// <summary>
    ///   Represents the source of a log event (such as a class, method or whatever makes sense).
    /// </summary>
    public sealed class LogEventSource : IStringValue
    {
        public string StringValue { get; }


        public override string ToString() => StringValue;

        public bool IsRetainedSection => StringValue == "__retained_section__";

        internal static LogEventSource RetainedSection() => new("__retained_section__");

        bool Equals(IStringValue other)
        {
            return StringValue == other.StringValue;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((LogEventSource)obj);
        }

        public static implicit operator LogEventSource?(string? stringValue) => stringValue.IsAssigned() 
            ? new(stringValue!) 
            : null;

        public static implicit operator string(LogEventSource logEventSource) => logEventSource.StringValue;

        public override int GetHashCode()
        {
            return StringValue.GetHashCode();
        }

        public static bool operator ==(LogEventSource? left, LogEventSource? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LogEventSource? left, LogEventSource? right)
        {
            return !Equals(left, right);
        }

        public LogEventSource(
            [CallerMemberName] string? stringValue = null, 
            [CallerFilePath] string? callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            StringValue = $"{stringValue}@{callerFile} (#{callerLine})";
        }

        internal LogEventSource(string stringValue)
        {
            StringValue = $"{stringValue}";
        }
    }
}