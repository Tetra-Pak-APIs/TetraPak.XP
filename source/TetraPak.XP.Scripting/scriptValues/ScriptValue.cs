using System;

namespace TetraPak.XP.Scripting
{
    public abstract class ScriptValue : IStringValue
    {
        public abstract bool IsCollection { get; }
        
        public string StringValue { get; }

        public static void AddProviders(params ScriptValueParser[] providers)
        {
            ScriptValueParser.AddProviders(providers);
        }

        public abstract bool Equals(ScriptValue value, StringComparison comparison);

        public abstract bool NotEquals(ScriptValue value, StringComparison comparison);

        public abstract bool Contains(ScriptValue value, StringComparison comparison);

        public abstract bool NotContains(ScriptValue value, StringComparison comparison);
        
        public abstract bool Contained(ScriptValue value, StringComparison comparison);

        public abstract bool NotContained(ScriptValue value, StringComparison comparison);
        
        public abstract bool LessThan(ScriptValue value, StringComparison comparison);

        public abstract bool LessThanOrEquals(ScriptValue value, StringComparison comparison);
        
        public abstract bool GreaterThan(ScriptValue value, StringComparison comparison);

        public abstract bool GreaterThanOrEquals(ScriptValue value, StringComparison comparison);
        
        public ScriptValue(string stringValue)
        {
            StringValue = stringValue;
        }

    }
}