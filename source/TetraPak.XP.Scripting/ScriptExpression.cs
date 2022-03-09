using System;
using System.Linq;
using System.Text;

namespace TetraPak.XP.Scripting
{
    /// <summary>
    ///   Abstracts a HTTP expression 
    /// </summary>
    public abstract class ScriptExpression : StringValueBase
    {
        internal new static Outcome<ScriptExpression> Parse(string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
                return Outcome<ScriptExpression>.Fail(new Exception("HTTP criteria expression was unassigned"));

            if (containsLogicalOperators(stringValue))
            {
                var logicalExprOutcome = parseLogicExpression(stringValue);
                return logicalExprOutcome
                    ? Outcome<ScriptExpression>.Success(logicalExprOutcome.Value!)
                    : Outcome<ScriptExpression>.Fail(logicalExprOutcome.Exception!);
            }

            var comparisonExpressionOutcome = parseComparisonExpression(stringValue); //  expression =  new ScriptComparisonExpression(stringValue);
            return comparisonExpressionOutcome
                ? Outcome<ScriptExpression>.Success(comparisonExpressionOutcome.Value!)
                : Outcome<ScriptExpression>.Fail(comparisonExpressionOutcome.Exception!);
            
            // return ! expression.IsError 
            //     ? Outcome<ScriptExpression>.Success(expression) 
            //     : Outcome<ScriptExpression>.Fail(
            //         new FormatException($"Invalid HTTP criteria expression: {stringValue}"));
        }

        internal abstract ScriptExpression Invert();

        public abstract bool IsMatch(StringComparison comparison);
        
        static Outcome<ScriptComparisonExpression> parseComparisonExpression(string stringValue)
        {
            var ca = stringValue.Trim().ToCharArray();
            var idx = 1;
            eatToTokens(ca, ref idx, out var left, out var opToken, ScriptTokens.ComparativeOperatorTokens);
            if (idx == ca.Length)
                return Outcome<ScriptComparisonExpression>.Fail(
                    new FormatException($"Expected comparison operator in expression '{stringValue}'"));

            // var operation = token!.ToOperator();
            var right = stringValue.Substring(idx).Trim();
            ScriptValue? leftValue = null;
            ScriptValue? rightValue = null;
            ComparativeOperation? operation = null;
            for (var i = 0; i < ScriptValueParser.ValueParsers.Count; i++)
            {
                var parser = ScriptValueParser.ValueParsers[i];
                if (leftValue is null)
                {
                    var outcome = parser.ParseLeftOperand(left, opToken!);
                    leftValue = outcome ? outcome.Value!.Value : null;
                    operation = outcome.Value!.Operation;
                }
                if (rightValue is null)
                {
                    var outcome = parser.ParseRightOperand(right, opToken!, operation);
                    rightValue = outcome ? outcome.Value!.Value : null;
                    operation = outcome.Value!.Operation;
                }
                if (leftValue is {} && rightValue is {})
                    break;
            }
            
            if (leftValue is null)
                return Outcome<ScriptComparisonExpression>.Fail($"Unrecognized left operand in '{stringValue}'");

            if (rightValue is null)
                return Outcome<ScriptComparisonExpression>.Fail($"Unrecognized right operand in '{stringValue}'");
            
            return Outcome<ScriptComparisonExpression>.Success(new ScriptComparisonExpression(stringValue, leftValue, rightValue));
        }
        
        static Outcome<ScriptLogicExpression> parseLogicExpression(string stringValue)
        {
            // yyy != xxx && !  (xxx == yyy && (xxx != yyy || xxx == yyy)) && yyy != xxx
            var ca = stringValue.Trim().ToCharArray();
            var idx = 0;
            ScriptLogicOperator logOp;
            var groupOutcome = tryEatGroupExpression(ca, ref idx, out var op, stringValue);
            ScriptLogicExpression logicExpr;
            if (groupOutcome)
            {
                logicExpr = groupOutcome.Value!;
            }
            else
            {
                var pos = idx;
                eatToTokens(ca, ref idx, out var sLeft, out op, ScriptTokens.AndOp, ScriptTokens.OrOp);
                if (op is null)
                    return Outcome<ScriptLogicExpression>.Fail(
                        new FormatException($"Invalid expression (at pos. {pos}): \"{stringValue}\""));

                var leftOutcome = parseComparisonExpression(sLeft);
                if (!leftOutcome)
                    return Outcome<ScriptLogicExpression>.Fail(
                        new FormatException($"Invalid expression (at pos. {pos}): \"{stringValue}\""));

                var left = leftOutcome.Value!;

                idx += 2;
                logOp = op == ScriptTokens.AndOp ? ScriptLogicOperator.And : ScriptLogicOperator.Or;
                skipWhite(ca, ref idx);
                groupOutcome = tryEatGroupExpression(ca, ref idx, out op, stringValue);
                ScriptExpression right;
                if (groupOutcome)
                {
                    right = groupOutcome.Value!;
                    logicExpr = new ScriptLogicExpression($"{left} {logOp.ToStringToken()} {right}")
                        .WithOperation(left, logOp, right);
                }
                else
                {
                    eatToTokens(ca, ref idx, out var sRight, out op, ScriptTokens.AndOp, ScriptTokens.OrOp);
                    if (sRight.Length == 0)
                        return Outcome<ScriptLogicExpression>.Fail(
                            new FormatException(
                                $"Invalid expression (at pos. {pos}): \"{stringValue}\". Expected right operand"));

                    var rightOutcome = parseComparisonExpression(sRight);
                    if (!rightOutcome)
                        return Outcome<ScriptLogicExpression>.Fail(
                            new FormatException($"Invalid expression (at pos. {pos}): \"{stringValue}\""));

                    right = rightOutcome.Value!;

                    logicExpr = new ScriptLogicExpression($"{left} {logOp.ToStringToken()} {right}")
                        .WithOperation(left, logOp, right);
                }
            }

            while (idx < ca.Length && op is {})
            {
                idx += 2;
                var pos = idx;
                logOp = op == ScriptTokens.AndOp ? ScriptLogicOperator.And : ScriptLogicOperator.Or;
                groupOutcome = tryEatGroupExpression(ca, ref idx, out op, stringValue);
                ScriptExpression right;
                if (groupOutcome)
                {
                    right = groupOutcome.Value!;
                }
                else
                {
                    eatToTokens(ca, ref idx, out var sRight, out op, ScriptTokens.AndOp, ScriptTokens.OrOp);
                    if (sRight.Length == 0)
                        return Outcome<ScriptLogicExpression>.Fail(
                            new FormatException(
                                $"Invalid expression (at pos. {pos}): \"{stringValue}\". Expected right operand"));

                    var rightOutcome = parseComparisonExpression(sRight);
                    if (!rightOutcome)
                        return Outcome<ScriptLogicExpression>.Fail(
                            new FormatException($"Invalid expression (at pos. {pos}): \"{stringValue}\""));

                    right = rightOutcome.Value!;
                }

                logicExpr = logicExpr.Expand(logOp, right);
            }
            return Outcome<ScriptLogicExpression>.Success(logicExpr);
        }
        
        static Outcome<ScriptLogicExpression> tryEatGroupExpression(char[] ca, ref int idx, out string? opToken, string stringValue)
        {
            var c = ca[idx];
            opToken = null;
            var isInverted = c == ScriptTokens.NotOp;
            string groupedExpression;
            Outcome<ScriptLogicExpression> groupOutcome;
            if (isInverted)
            {
                ++idx;
                if (!trySkipWhiteToExpected(ca, ref idx, ScriptTokens.GroupPrefix))
                    return Outcome<ScriptLogicExpression>.Fail(
                        new FormatException($"Invalid HTTP criteria (at pos. {idx}): \"{stringValue}\". Expected group expression at"));

                ++idx;
                if (!tryEatGroup(ca, ref idx, out groupedExpression))
                    return Outcome<ScriptLogicExpression>.Fail(
                        new FormatException($"Invalid HTTP criteria (at pos. {idx}): \"{stringValue}\". Expected closing group suffix at {idx}"));

                groupOutcome = parseLogicExpression(groupedExpression);
                if (!groupOutcome)
                    return groupOutcome;

                var expression = groupOutcome.Value!;
                var invertedExpression = (ScriptLogicExpression) expression.Invert();
                eatToTokens(ca, ref idx, out _, out opToken, ScriptTokens.AndOp, ScriptTokens.OrOp);
                if (expression.IsComplete)
                    return Outcome<ScriptLogicExpression>.Success(invertedExpression);
            }

            if (c != ScriptTokens.GroupPrefix)
                return Outcome<ScriptLogicExpression>.Fail(new Exception("Not a grouped expression"));
            
            ++idx;
            if (!tryEatGroup(ca, ref idx, out groupedExpression))
                return Outcome<ScriptLogicExpression>.Fail(
                    new FormatException($"Expected group balanced suffix at {idx}"));
            
            groupOutcome = parseLogicExpression(groupedExpression);
            eatToTokens(ca, ref idx, out _, out opToken, ScriptTokens.AndOp, ScriptTokens.OrOp);
            return !groupOutcome 
                ? groupOutcome 
                : Outcome<ScriptLogicExpression>.Fail(new Exception("Not a grouped expression"));
        }

        static bool isToken(string token, char[] ca, int idx)
        {
            if (ca[idx] != token[0] || idx+token.Length > ca.Length)
                return false;

            var caToken = token.ToCharArray();
            for (var i = 1; i < caToken.Length; i++)
            {
                if (ca[idx + i] != caToken[i])
                    return false;
            }

            return true;
        }

        static bool isAnyToken(string[] tokens, char[] ca, int idx, out string? token)
        {
            token = tokens.FirstOrDefault(t => isToken(t, ca, idx));
            return !string.IsNullOrEmpty(token);
        }

        static bool trySkipWhiteToExpected(char[] ca, ref int index, char expected)
        {
            var i = index;
            var c = ca[i];
            for (; i < ca.Length && char.IsWhiteSpace(c); i++)
            {
                c = ca[i];
            }
            index = i;
            return c == expected;
        }

        static void skipWhite(char[] ca, ref int index)
        {
            var i = index;
            var c = ca[i];
            for (; i < ca.Length && char.IsWhiteSpace(c); i++)
            {
                c = ca[i];
            }
            index = i-1;
        }

        static void eatToTokens(char[] ca, ref int index, out string text, out string? token, params string[] tokens)
        {
            var i = index;
            var sb = new StringBuilder();
            for (; i < ca.Length; i++)
            {
                var c = ca[i];
                for (var x = 0; x < tokens.Length; x++)
                {
                    token = tokens[x];
                    if (!isToken(token, ca, i)) 
                        continue;
                    
                    index = i;
                    text = sb.ToString().Trim();
                    return;
                }

                sb.Append(c);
            }

            token = null;
            index = i;
            text = sb.ToString().Trim();
        }

        static bool tryEatGroup(char[] ca, ref int index, out string groupedExpression)
        {
            var i = index;
            var count = 1;
            var sb = new StringBuilder();
            for (; i < ca.Length; i++)
            {
                var c = ca[i];
                switch (c)
                {
                    case ScriptTokens.GroupSuffix:
                    {
                        count--;
                        if (count > 0)
                        {
                            sb.Append(c);
                            continue;
                        }

                        groupedExpression = sb.ToString();
                        index = i;
                        return true;
                    }
                    case ScriptTokens.GroupPrefix:
                        count++;
                        break;
                }

                sb.Append(c);
            }

            groupedExpression = sb.ToString();
            return false;
        }

        static bool containsLogicalOperators(string stringValue)
        {
            return stringValue.Contains(ScriptTokens.AndOp) || stringValue.Contains(ScriptTokens.OrOp);
        }

        protected ScriptExpression(string? stringValue) : base(stringValue)
        {
        }
    }
}