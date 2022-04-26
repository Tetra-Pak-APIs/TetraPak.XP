using System;
using System.Text;
using System.Threading.Tasks;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Scripting
{
    /// <summary>
    ///   Abstracts a HTTP expression 
    /// </summary>
    public abstract class ScriptExpression : StringValueBase
    {
        protected IScriptContext? Context { get; set; }

        ScriptExpression withContext(IScriptContext? context)
        {
            Context = context;
            return this;
        }

        public static async Task<Outcome<ScriptExpression>> ParseAsync(string stringValue,
            IScriptContext? context = null)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
                return Errors.UnassignedScript();

            if (containsLogicalOperators(stringValue))
            {
                var logicalExprOutcome = await parseLogicExpressionAsync(stringValue);
                return logicalExprOutcome
                    ? Outcome<ScriptExpression>.Success(logicalExprOutcome.Value!.withContext(context))
                    : Outcome<ScriptExpression>.Fail(logicalExprOutcome.Exception!);
            }

            var comparisonExpressionOutcome = await parseComparisonExpressionAsync(stringValue);
            return comparisonExpressionOutcome
                ? Outcome<ScriptExpression>.Success(comparisonExpressionOutcome.Value!.withContext(context))
                : Outcome<ScriptExpression>.Fail(comparisonExpressionOutcome.Exception!);
        }

        internal abstract ScriptExpression Invert();

        // /// <summary>
        // ///   Runs a scripted expression.
        // /// </summary>
        // /// <param name="expression">
        // /// </param>
        // /// <param name="context">
        // ///   (optional)<br/>
        // ///   A script context to be used for the execution.
        // /// </param>
        // /// <returns>
        // ///   An <see cref="Outcome"/> to indicate success/failure.  
        // /// </returns>
        // public static Outcome Run(string expression, IScriptContext? context = null)
        // {
        //     
        //
        // }
        //
        // /// <summary>
        // ///   Runs a scripted expression to produce a value of an expected type.
        // /// </summary>
        // /// <param name="expression">
        // /// </param>
        // /// <param name="context">
        // ///   (optional)<br/>
        // ///   A script context to be used for the execution.
        // /// </param>
        // /// <typeparam name="T">
        // ///   The expected return type.
        // /// </typeparam>
        // /// <returns>
        // ///   An <see cref="Outcome"/> to indicate success/failure and, on success, also carry
        // ///   a value of the expected type (<typeparamref name="T"/>), on failure, <see cref="Outcome.Exception"/> and <see cref="Outcome.Message"/>.  
        // /// </returns>
        // /// <seealso cref="RunAsync{T}"/>
        // public static Outcome<T> Run<T>(string expression, IScriptContext? context = null)
        // {
        //     var parseOutcome = ParseAsync(expression);
        //     if (!parseOutcome)
        //         return Outcome<T>.Fail(parseOutcome.Exception!);
        //
        //     var outcome = parseOutcome.Value!.Run(context);
        //     return outcome && outcome is Outcome<T> typedOutcome
        //         ? typedOutcome
        //         : Outcome<T>.Fail(outcome.Exception!);
        // }

        /// <summary>
        ///   Runs a scripted expression in a background thread.
        /// </summary>
        /// <param name="expression">
        /// </param>
        /// <param name="context">
        ///   (optional)<br/>
        ///   A script context to be used for the execution.
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome"/> to indicate success/failure.  
        /// </returns>
        /// <seealso cref="RunAsync{T}(string,IScriptContext?)"/>
        public static Task<Outcome> RunAsync(string expression, IScriptContext? context = null)
            =>
                Task.Run(async () =>
                {
                    var parseOutcome = await ParseAsync(expression);
                    if (!parseOutcome)
                        return Outcome.Fail(parseOutcome.Exception!);

                    return await parseOutcome.Value!.RunAsync(context);
                });

        /// <summary>
        ///   Runs a scripted expression in a background thread to produce a value of an expected type.
        /// </summary>
        /// <param name="expression">
        /// </param>
        /// <param name="context">
        ///   (optional)<br/>
        ///   A script context to be used for the execution.
        /// </param>
        /// <typeparam name="T">
        ///   The expected return type.
        /// </typeparam>
        /// <returns>
        ///   An <see cref="Outcome"/> to indicate success/failure and, on success, also carry
        ///   a value of the expected type (<typeparamref name="T"/>), on failure, <see cref="Outcome.Exception"/> and <see cref="Outcome.Message"/>.  
        /// </returns>
        /// <seealso cref="RunAsync(string,IScriptContext?)"/>
        public static async Task<Outcome<T>> RunAsync<T>(string expression, IScriptContext? context = null)
        {
            var outcome = await RunAsync(expression, context);
            return outcome && outcome is Outcome<T> typedOutcome
                ? typedOutcome
                : Outcome<T>.Fail(outcome.Exception!);
        }

        // public abstract Outcome Run(IScriptContext? context = null);

        public abstract Task<Outcome> RunAsync(IScriptContext? context = null); // => Task.Run(() => Run(context)); 

        static async Task<Outcome<ScriptComparisonExpression>> parseComparisonExpressionAsync(string stringValue)
        {
            var ca = stringValue.Trim().ToCharArray();
            var idx = 0;
            eatToTokens(ca, ref idx, out var left, out var opToken, ScriptTokens.ComparativeOperatorTokens);
            if (idx == ca.Length)
                return Errors.ExpectedComparativeOperator(stringValue);

            // var operation = token!.ToOperator();
            var right = stringValue.Substring(idx).Trim();
            ScriptValue? leftValue = null;
            ScriptValue? rightValue = null;
            ComparativeOperation? operation = null;
            var parsers = ScriptValueParser.ValueParsers;
            for (var i = 0; i < parsers.Length; i++)
            {
                var parser = parsers[i];
                if (leftValue is null)
                {
                    var outcome = parser.ParseLeftOperand(left, opToken!);
                    if (!outcome && outcome.Exception is ScriptException)
                        return Outcome<ScriptComparisonExpression>.Fail(outcome.Exception!);

                    leftValue = outcome ? outcome.Value!.Value : null;
                    operation = outcome ? outcome.Value!.Operation : operation;
                }

                if (rightValue is null)
                {
                    var outcome = parser.ParseRightOperand(right, opToken!, operation);
                    if (!outcome && outcome.Exception is ScriptException)
                        return Outcome<ScriptComparisonExpression>.Fail(outcome.Exception);

                    rightValue = outcome ? outcome.Value!.Value : null;
                    operation = outcome ? outcome.Value!.Operation : operation;
                }

                if (leftValue is null || rightValue is null || operation is null)
                    continue;

                if (!await leftValue.IsValidOperationAsync(operation.Value) ||
                    !await rightValue.IsValidOperationAsync(operation.Value.FromRightOperandPerspective()))
                    return Errors.InvalidOperation<ScriptComparisonExpression>(opToken!, leftValue, rightValue);

                break;
            }

            if (operation is null)
                return Errors.UnrecognizedComparativeOperation(stringValue);

            if (leftValue is null)
                return Errors.UnrecognizedLeftOperand(stringValue);

            if (rightValue is null)
                return Errors.UnrecognizedRightOperand(stringValue);

            return Outcome<ScriptComparisonExpression>.Success(
                new ScriptComparisonExpression(stringValue, leftValue, rightValue, operation.Value));
        }

        static async Task<Outcome<ScriptLogicExpression>> parseLogicExpressionAsync(string stringValue)
        {
            // yyy != xxx && !  (xxx == yyy && (xxx != yyy || xxx == yyy)) && yyy != xxx
            var ca = stringValue.Trim().ToCharArray();
            var idx = 0;
            ScriptLogicOperator logOp;
            var groupOutcome = await tryEatGroupExpressionAsync(ca, idx, stringValue);
            // var groupOutcome =  tryEatGroupExpressionAsync(ca, ref idx, out var op, stringValue); obsolete
            ScriptLogicExpression logicExpr;
            string? op;
            if (groupOutcome)
            {
                logicExpr = groupOutcome.Value!.Expression;
                idx = groupOutcome.Value!.Index;
                op = groupOutcome.Value!.OperatorToken;
            }
            else
            {
                var pos = idx;
                eatToTokens(ca, ref idx, out var sLeft, out op, ScriptTokens.AndOp, ScriptTokens.OrOp);
                if (op is null)
                    return Outcome<ScriptLogicExpression>.Fail(
                        new FormatException($"Invalid expression (at pos. {pos}): \"{stringValue}\""));

                var leftOutcome = await parseComparisonExpressionAsync(sLeft);
                if (!leftOutcome)
                    return Outcome<ScriptLogicExpression>.Fail(
                        new FormatException($"Invalid expression (at pos. {pos}): \"{stringValue}\""));

                var left = leftOutcome.Value!;

                idx += 2;
                logOp = op == ScriptTokens.AndOp ? ScriptLogicOperator.And : ScriptLogicOperator.Or;
                skipWhite(ca, ref idx);
                groupOutcome = await tryEatGroupExpressionAsync(ca, idx, stringValue);
                // groupOutcome = tryEatGroupExpressionAsync(ca, ref idx, out op, stringValue); obsolete
                ScriptExpression right;
                if (groupOutcome)
                {
                    right = groupOutcome.Value!.Expression;
                    idx = groupOutcome.Value!.Index;
                    op = groupOutcome.Value!.OperatorToken;
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

                    var rightOutcome = await parseComparisonExpressionAsync(sRight);
                    if (!rightOutcome)
                        return Outcome<ScriptLogicExpression>.Fail(
                            new FormatException($"Invalid expression (at pos. {pos}): \"{stringValue}\""));

                    right = rightOutcome.Value!;

                    logicExpr = new ScriptLogicExpression($"{left} {logOp.ToStringToken()} {right}")
                        .WithOperation(left, logOp, right);
                }
            }

            while (idx < ca.Length && op is { })
            {
                idx += 2;
                var pos = idx;
                logOp = op == ScriptTokens.AndOp ? ScriptLogicOperator.And : ScriptLogicOperator.Or;
                groupOutcome = await tryEatGroupExpressionAsync(ca, idx, stringValue);
                // groupOutcome = tryEatGroupExpressionAsync(ca, ref idx, out op, stringValue); obsolete
                ScriptExpression right;
                if (groupOutcome)
                {
                    right = groupOutcome.Value!.Expression;
                    idx = groupOutcome.Value!.Index;
                    op = groupOutcome.Value!.OperatorToken;
                }
                else
                {
                    eatToTokens(ca, ref idx, out var sRight, out op, ScriptTokens.AndOp, ScriptTokens.OrOp);
                    if (sRight.Length == 0)
                        return Outcome<ScriptLogicExpression>.Fail(
                            new FormatException(
                                $"Invalid expression (at pos. {pos}): \"{stringValue}\". Expected right operand"));

                    var rightOutcome = await parseComparisonExpressionAsync(sRight);
                    if (!rightOutcome)
                        return Outcome<ScriptLogicExpression>.Fail(
                            new FormatException($"Invalid expression (at pos. {pos}): \"{stringValue}\""));

                    right = rightOutcome.Value!;
                }

                logicExpr = logicExpr.Expand(logOp, right);
            }

            return Outcome<ScriptLogicExpression>.Success(logicExpr);
        }

        static async Task<Outcome<EatGroupExpressionInfo>> tryEatGroupExpressionAsync(char[] ca, int idx,
            string stringValue)
        {
            var c = ca[idx];
            string? opToken = null;
            var isInverted = c == ScriptTokens.NotOp;
            string groupedExpression;
            Outcome<ScriptLogicExpression> groupOutcome;
            if (isInverted)
            {
                ++idx;
                if (!trySkipWhiteToExpected(ca, ref idx, ScriptTokens.GroupPrefix))
                    return Outcome<EatGroupExpressionInfo>.Fail(
                        new FormatException(
                            $"Invalid HTTP criteria (at pos. {idx}): \"{stringValue}\". Expected group expression at"));

                ++idx;
                if (!tryEatGroup(ca, ref idx, out groupedExpression))
                    return Outcome<EatGroupExpressionInfo>.Fail(
                        new FormatException(
                            $"Invalid HTTP criteria (at pos. {idx}): \"{stringValue}\". Expected closing group suffix at {idx}"));

                groupOutcome = await parseLogicExpressionAsync(groupedExpression);
                if (!groupOutcome)
                    return Outcome<EatGroupExpressionInfo>.Success(new EatGroupExpressionInfo(groupOutcome.Value!, idx,
                        opToken)); // groupOutcome;

                var expression = groupOutcome.Value!;
                var invertedExpression = (ScriptLogicExpression)expression.Invert();
                eatToTokens(ca, ref idx, out _, out opToken, ScriptTokens.AndOp, ScriptTokens.OrOp);
                if (expression.IsComplete)
                    return Outcome<EatGroupExpressionInfo>.Success(
                        new EatGroupExpressionInfo(invertedExpression, idx, opToken!));
                //  return Outcome<ScriptLogicExpression>.Success(invertedExpression);
            }

            if (c != ScriptTokens.GroupPrefix)
                return Outcome<EatGroupExpressionInfo>.Fail(new Exception("Not a grouped expression"));
            //  return Outcome<ScriptLogicExpression>.Fail(new Exception("Not a grouped expression"));

            ++idx;
            if (!tryEatGroup(ca, ref idx, out groupedExpression))
                return Outcome<EatGroupExpressionInfo>.Fail(
                    new FormatException($"Expected group balanced suffix at {idx}"));
            // return Outcome<ScriptLogicExpression>.Fail(new FormatException($"Expected group balanced suffix at {idx}"));

            groupOutcome = await parseLogicExpressionAsync(groupedExpression);
            eatToTokens(ca, ref idx, out _, out opToken, ScriptTokens.AndOp, ScriptTokens.OrOp);
            return !groupOutcome
                ? Outcome<EatGroupExpressionInfo>.Success(
                    new EatGroupExpressionInfo(groupOutcome.Value!, idx, opToken!))
                : Outcome<EatGroupExpressionInfo>.Fail(new Exception("Not a grouped expression"));
            // : Outcome<ScriptLogicExpression>.Fail(new Exception("Not a grouped expression"));
        }

        sealed class EatGroupExpressionInfo
        {
            public ScriptLogicExpression Expression { get; }

            public int Index { get; }

            public string OperatorToken { get; }

            public EatGroupExpressionInfo(ScriptLogicExpression expression, int index, string operatorToken)
            {
                Expression = expression;
                Index = index;
                OperatorToken = operatorToken;
            }
        }

        static bool isToken(string token, char[] ca, int idx)
        {
            if (ca[idx] != token[0] || idx + token.Length > ca.Length)
                return false;

            var caToken = token.ToCharArray();
            for (var i = 1; i < caToken.Length; i++)
            {
                if (ca[idx + i] != caToken[i])
                    return false;
            }

            return true;
        }

        // static bool isAnyToken(string[] tokens, char[] ca, int idx, out string? token) obsolete
        // {
        //     token = tokens.FirstOrDefault(t => isToken(t, ca, idx));
        //     return !string.IsNullOrEmpty(token);
        // }

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

            index = i - 1;
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

                    index = i + token.Length;
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