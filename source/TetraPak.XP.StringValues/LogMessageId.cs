
namespace TetraPak.XP.StringValues
{
    /// <summary>
    ///   A <see cref="IStringValue"/> representing a log message id, used for tracking related events. 
    /// </summary>
    public sealed class LogMessageId : IStringValue
    {
        public string StringValue { get; }

        public static implicit operator string?(LogMessageId? messageId) => messageId?.StringValue;

        public static implicit operator LogMessageId?(string? stringValue) => stringValue.IsUnassigned() 
            ? null 
            : new LogMessageId(stringValue!);
        
        LogMessageId(string stringValue)
        {
            StringValue = stringValue;
        }
    }
}