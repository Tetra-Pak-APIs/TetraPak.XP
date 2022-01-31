
namespace TetraPak.XP.Logging
{
    public class LogMessageId : IStringValue
    {
        public string StringValue { get; }

        public static implicit operator string(LogMessageId messageId) => messageId.StringValue;

        public static explicit operator LogMessageId(string stringValue) => new(stringValue);
        
        LogMessageId(string stringValue)
        {
            StringValue = stringValue;
        }
    }
}