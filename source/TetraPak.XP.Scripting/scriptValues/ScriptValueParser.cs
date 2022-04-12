using System.Collections.Generic;

namespace TetraPak.XP.Scripting;

public abstract class ScriptValueParser
{
    static readonly List<ScriptValueParser> s_valueParsers;

    internal static ScriptValueParser[] ValueParsers => s_valueParsers.ToArray();

    public static Outcome<ScriptValue> TryParseValue(string stringValue)
    {
        var parsers = ValueParsers;
        for (var i = 0; i < parsers.Length; i++)
        {
            var parser = parsers[i];
            var outcome = parser.ParseValue(stringValue);
            if (outcome)
                return outcome;
        }
        
        return Errors.CannotResolveValue(stringValue);
    }

    protected abstract Outcome<ScriptValue> ParseValue(string stringValue);

    public IScriptContext? Context { get; set; }
    
    internal static void AddProviders(params ScriptValueParser[] providers) => s_valueParsers.AddRange(providers);

    public abstract Outcome<ParseOperandResult> ParseLeftOperand(
        string stringValue, 
        string operatorToken);

    public abstract Outcome<ParseOperandResult> ParseRightOperand(
        string stringValue, 
        string operatorToken,
        ComparativeOperation? suggestedOperation);

    static ScriptValueParser()
    {
        s_valueParsers = new List<ScriptValueParser>(new[]
        {
            new LiteralsParser()
        });
    }

}

public sealed class ParseOperandResult
{
    public ScriptValue Value { get; }

    public ComparativeOperation Operation { get; }

    public ParseOperandResult(ScriptValue value, ComparativeOperation operation)
    {
        Value = value;
        Operation = operation;
    }
}

public static class ScriptValueParserHelper
{
    public static ScriptValueParser WithContext(this ScriptValueParser parser, IScriptContext context)
    {
        parser.Context = context;
        return parser;
    }
}