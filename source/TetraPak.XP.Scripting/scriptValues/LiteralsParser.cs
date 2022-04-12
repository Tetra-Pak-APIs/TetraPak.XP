using System;
using System.Globalization;
using TetraPak.XP;
using TetraPak.XP.Scripting;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Scripting
{
    sealed class LiteralsParser : ScriptValueParser
    {
        protected override Outcome<ScriptValue> ParseValue(string stringValue)
        {
            if (isStringLiteral(stringValue))
                return Outcome<ScriptValue>.Success(new StringLiteralValue(stringValue));

            if (isNumericLiteral(stringValue, out var value))
                return Outcome<ScriptValue>.Success(new NumericLiteralValue(stringValue, value));

            return Outcome<ScriptValue>.Fail("not literal");
        }

        public override Outcome<ParseOperandResult> ParseLeftOperand(string stringValue, string operatorToken)
        {
            // todo support more value types (such as objects)
            return parseLiteral(stringValue, operatorToken, null);
        }

        public override Outcome<ParseOperandResult> ParseRightOperand(string stringValue, string operatorToken, ComparativeOperation? suggestedOperation)
        {
            // todo support more value types (such as objects)
            return parseLiteral(stringValue, operatorToken, suggestedOperation);
        }

        static Outcome<ParseOperandResult> parseLiteral(
            string stringValue, 
            string operatorToken,
            ComparativeOperation? suggestedOperation)
        {
            if (isNullLiteral(stringValue))
                return Outcome<ParseOperandResult>.Success(new ParseOperandResult(
                    new NullLiteralValue(stringValue), 
                    suggestedOperation ?? operatorToken.ToComparativeOperator()));
            
            if (isStringLiteral(stringValue))
                return Outcome<ParseOperandResult>.Success(new ParseOperandResult(
                    new StringLiteralValue(stringValue), 
                     suggestedOperation ?? operatorToken.ToComparativeOperator()));

            if (isNumericLiteral(stringValue, out var value))
                return Outcome<ParseOperandResult>.Success(new ParseOperandResult(
                    new NumericLiteralValue(stringValue, value), 
                    suggestedOperation ?? operatorToken.ToComparativeOperator()));
            
            // todo support other literals, such as DateTime, TimeSpan etc ...
            return Outcome<ParseOperandResult>.Fail("Unrecognized literal");
        }
        
        static bool isNullLiteral(string stringValue) => stringValue == NullLiteralValue.Identifier;

        static bool isStringLiteral(string stringValue)
        {
            return stringValue.StartsWith(ScriptTokens.StringLiteralQualifier) 
                   &&
                   stringValue.EndsWith(ScriptTokens.StringLiteralQualifier);
        }
        
        static bool isNumericLiteral(string stringValue, out double value)
        {
            var format = CultureInfo.InvariantCulture;
            return double.TryParse(stringValue, NumberStyles.Any, format, out value);
        }
    }
}

sealed class LiteralScriptValueFactory : IScriptValueFactory
{
    public Outcome<ScriptValue> GetScriptValue(string key, object? value)
    {
        if (value is null)
            return Outcome<ScriptValue>.Success(new NullLiteralValue(string.Empty));

        if (value.IsNumeric())
        {
            var d = Convert.ToDouble(value);
            return Outcome<ScriptValue>.Success(new NumericLiteralValue(value.ToString()!, d));
        }

        return value switch
        {
            string s => Outcome<ScriptValue>.Success(new StringLiteralValue(s)),
            IStringValue sv => Outcome<ScriptValue>.Success(new StringLiteralValue(sv.StringValue)),
            _ => Outcome<ScriptValue>.Fail("not supported")
        };
    }
}