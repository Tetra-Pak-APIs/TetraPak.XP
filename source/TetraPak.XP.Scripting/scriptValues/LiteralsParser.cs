using System.Globalization;
using System.Linq;

namespace TetraPak.XP.Scripting
{
    class LiteralsParser : ScriptValueParser
    {
        static readonly string[] s_stringOperators = {
            ScriptTokens.Equal,
            ScriptTokens.NotEqual,
            ScriptTokens.LessThan,
            ScriptTokens.LessThanOrEquals,
            ScriptTokens.GreaterThan,
            ScriptTokens.GreaterThanOrEquals
        };

        static readonly string[] s_numericOperators = {
            ScriptTokens.Equal,
            ScriptTokens.NotEqual,
            ScriptTokens.LessThan,
            ScriptTokens.LessThanOrEquals,
            ScriptTokens.GreaterThan,
            ScriptTokens.GreaterThanOrEquals
        };

        public override Outcome<ParseOperandResult> ParseLeftOperand(string stringValue, string operatorToken)
            =>
            parseLiteral(stringValue, operatorToken, null);

        public override Outcome<ParseOperandResult> ParseRightOperand(string stringValue, string operatorToken, ComparativeOperation? suggestedOperation)
            =>
            parseLiteral(stringValue, operatorToken, suggestedOperation);

        Outcome<ParseOperandResult> parseLiteral(string stringValue, string operatorToken,
            ComparativeOperation? suggestedOperation)
        {
            if (isStringLiteral(stringValue))
            {
                if (!s_stringOperators.Contains(operatorToken))
                    return Outcome<ParseOperandResult>.Fail("Unsupported operator");
                
                return Outcome<ParseOperandResult>.Success(new ParseOperandResult(
                    new StringLiteral(stringValue), 
                     suggestedOperation ?? operatorToken.ToOperator(true)));
            }

            if (isNumericLiteral(stringValue, out var numericValue))
            {
                if (!s_numericOperators.Contains(operatorToken))
                    return Outcome<ParseOperandResult>.Fail("Unsupported operator");
                    
                return Outcome<ParseOperandResult>.Success(new ParseOperandResult(
                    new NumericLiteral(stringValue, numericValue), 
                    suggestedOperation ?? operatorToken.ToOperator(false)));
            }
            
            // todo support other literals, such as DateTime, TimeSpan etc ...
            return Outcome<ParseOperandResult>.Fail("Unrecognized literal");
        }

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