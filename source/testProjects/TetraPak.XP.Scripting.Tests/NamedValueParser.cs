using TetraPak.XP.StringValues;

namespace TetraPak.XP.Scripting.Tests;

sealed class NamedValueParser : ScriptValueParser
{
    const char IndexPrefix = '[';
    const char IndexSuffix = ']';
    const char MemberQualifier = '.';

    protected override Outcome<ScriptValue> ParseValue(string stringValue)
    {
        return parseValueReference(stringValue);
    }

    public override Outcome<ParseOperandResult> ParseLeftOperand(string stringValue, string operatorToken)
    {
        var outcome = parseValueReference(stringValue); 
        if (outcome)
            return Outcome<ParseOperandResult>.Success(new ParseOperandResult(
                outcome.Value!,
                operatorToken.ToComparativeOperator()));
        
        return Outcome<ParseOperandResult>.Fail("not an object");
    }

    public override Outcome<ParseOperandResult> ParseRightOperand(string stringValue, string operatorToken, ComparativeOperation? suggestedOperation)
    {
        var outcome = parseValueReference(stringValue); 
        if (outcome)
            return Outcome<ParseOperandResult>.Success(new ParseOperandResult(
                outcome.Value!,
                operatorToken.ToComparativeOperator()));
        
        return Outcome<ParseOperandResult>.Fail("not an object");
    }

    static Outcome<ScriptValueRef> parseValueReference(string stringValue)
    {
        var items = new MultiStringValue(stringValue, ".");
        ScriptValueRef? root = null;
        ScriptValueRef? declaring = null;
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            // ex: someValue[someValue.Length]
            var indexedOutcome = parseIndexed(items[0]);
            var info = indexedOutcome.Value;
            var identifier = info?.MemberIdentifier ?? item;
            var scriptValue = new ScriptValueRef(stringValue, identifier, info?.IndexValue);
            root ??= scriptValue;
            declaring?.SetMember(scriptValue);
            declaring = scriptValue;
        }
        return Outcome<ScriptValueRef>.Success(root!);
        // abc
        // abc.def
        // abc[x]
    }

    static Outcome<IndexedInfo> parseIndexed(string stringValue)
    {
        if (stringValue[^1] != IndexSuffix) 
            return Outcome<IndexedInfo>.Fail("not indexed");

        var idxStart = stringValue.LastIndexOf(IndexPrefix);
        if (idxStart == -1)
            return Outcome<IndexedInfo>.Fail("not indexed");

        var idxToken = stringValue.Substring(idxStart + 1, stringValue.Length - 2 - idxStart);  
        var idxValueOutcome = TryParseValue(idxToken);
        if (!idxValueOutcome)
            return Outcome<IndexedInfo>.Fail(idxValueOutcome.Exception!);

        var ident = stringValue.Substring(0, idxStart);
        var identOutcome = TryParseValue(ident);
        if (!identOutcome)
            return Outcome<IndexedInfo>.Fail(identOutcome.Exception!);

        return Outcome<IndexedInfo>.Success(new IndexedInfo(ident, idxValueOutcome.Value!));
    }

    sealed class IndexedInfo
    {
        public string MemberIdentifier { get; }

        public ScriptValue IndexValue { get; }
        
        public IndexedInfo(string memberIdentifier, ScriptValue indexValue)
        {
            MemberIdentifier = memberIdentifier;
            IndexValue = indexValue;
        }
    }
}