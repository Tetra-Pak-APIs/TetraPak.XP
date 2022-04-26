using System;

namespace TetraPak.XP.Scripting
{
    public static class Errors
    {

        public const int CodeUnassignedScript = 1000;
        public const int CodeInvalidOperator = 2000;
        public const int CodeUnrecognizedComparativeExpression = 2001;
        public const int CodeUnrecognizedLeftOperand = 2002;
        public const int CodeUnrecognizedRightOperand = 2003;
        public const int CodeExpectedComparisonOperator = 2004;
        public const int CodeUnresolvedSymbol = 2005;
        public const int CodeExpectedSymbol = 2006;

        internal static Outcome<ScriptExpression> UnassignedScript()
            => Outcome<ScriptExpression>.Fail(
                new ScriptException(CodeUnassignedScript, "Script was unassigned"));

        internal static Outcome<T> InvalidOperation<T>(string opToken, ScriptValue leftOperand,
            ScriptValue rightOperand)
            where T : ScriptExpression
            =>
                Outcome<T>.Fail(new ScriptException(CodeInvalidOperator,
                    $"Cannot apply operator '{opToken}' to operands of type '{leftOperand.ValueTypeIdentifier}' and '{rightOperand.ValueTypeIdentifier}'"));

        internal static Outcome<ScriptComparisonExpression> UnrecognizedComparativeOperation(string expression)
            =>
                Outcome<ScriptComparisonExpression>.Fail(new ScriptException(CodeUnrecognizedComparativeExpression,
                    $"Unrecognized comparative operation: '{expression}'"));

        public static Outcome<ScriptComparisonExpression> UnrecognizedLeftOperand(string expression)
        {
            return Outcome<ScriptComparisonExpression>.Fail(new ScriptException(CodeUnrecognizedLeftOperand,
                $"Unrecognized left operand: '{expression}'"));
        }

        public static Outcome<ScriptComparisonExpression> UnrecognizedRightOperand(string expression)
        {
            return Outcome<ScriptComparisonExpression>.Fail(new ScriptException(
                CodeUnrecognizedRightOperand,
                $"Unrecognized right operand: '{expression}'"));
        }

        public static Outcome<ScriptComparisonExpression> ExpectedComparativeOperator(string expression)
        {
            return Outcome<ScriptComparisonExpression>.Fail(new ScriptException(
                CodeExpectedComparisonOperator,
                $"Expected comparative operator in expression '{expression}'"));
        }

        public static Outcome<ScriptValue> CannotResolveValue(string stringValue)
        {
            return Outcome<ScriptValue>.Fail(new ScriptException(
                CodeUnresolvedSymbol,
                $"Cannot resolve symbol: '{stringValue}'"));
        }

        public static Outcome<T> ExpectedIndexKeySymbol<T>(string stringValue)
        {
            return Outcome<T>.Fail(new ScriptException(
                CodeExpectedSymbol,
                $"Expected indexer key but none was specified: '{stringValue}'"));
        }
    }

    public sealed class ScriptException : Exception
    {
        public int ErrorCode { get; set; }

        public ScriptException(int errorCode, string message, Exception? inner = null)
            : base(message, inner)
        {
            ErrorCode = errorCode;
        }
    }
}