using System.Collections.Generic;

namespace TetraPak.XP.Scripting
{
    public abstract class ScriptValueParser
    {
        internal static List<ScriptValueParser> ValueParsers { get; }
        
        internal static void AddProviders(params ScriptValueParser[] providers)
        {
            ValueParsers.AddRange(providers);
        }
        
        public abstract Outcome<ParseOperandResult> ParseLeftOperand(
            string stringValue, 
            string operatorToken);

        public abstract Outcome<ParseOperandResult> ParseRightOperand(
            string stringValue, 
            string operatorToken,
            ComparativeOperation? suggestedOperation);

        static ScriptValueParser()
        {
            ValueParsers = new List<ScriptValueParser>(new[]
            {
                new LiteralsParser()
            });
        }
    }

    public class ParseOperandResult
    {
        public ScriptValue Value { get; }

        public ComparativeOperation Operation { get; }

        public ParseOperandResult(ScriptValue value, ComparativeOperation operation)
        {
            Value = value;
            Operation = operation;
        }
    }
}