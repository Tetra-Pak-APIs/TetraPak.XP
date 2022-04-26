using System.Threading.Tasks;

namespace TetraPak.XP.Scripting
{
    /// <summary>
    ///   A string compatible (criteria) expression for use with HTTP requests.
    /// </summary>
    public sealed class ScriptComparisonExpression : ScriptExpression
    {
        /// <summary>
        ///   Specifies the comparative operator.
        /// </summary>
        /// <seealso cref="ComparativeOperation"/>
        public ComparativeOperation Operation { get; }

        public ScriptValue LeftOperand { get; }

        public ScriptValue RightOperand { get; }

        internal override ScriptExpression Invert()
        {
            var op = Operation.Invert();
            var stringValue = $"{LeftOperand}] {op.ToStringToken()} {RightOperand}";
            return new ScriptComparisonExpression(stringValue, LeftOperand, RightOperand, op);
        }


        public override async Task<Outcome> RunAsync(IScriptContext? context = null)
        {
            context ??= Context ?? ScriptContext.Default;
            ScriptValue.WithContext(context);
            ScriptValue.WithContext(context);

            switch (Operation)
            {
                case ComparativeOperation.Equal:
                    return await LeftOperand.Equals(RightOperand) ? Outcome.Success() : Outcome.Fail("not equal");

                case ComparativeOperation.NotEqual:
                    return await LeftOperand.NotEquals(RightOperand) ? Outcome.Success() : Outcome.Fail("equal");

                case ComparativeOperation.LessThan:
                    return await LeftOperand.LessThan(RightOperand) ? Outcome.Success() : Outcome.Fail("not less");

                case ComparativeOperation.LessThanOrEquals:
                    return await LeftOperand.LessThanOrEquals(RightOperand)
                        ? Outcome.Success()
                        : Outcome.Fail("not less or equal");

                case ComparativeOperation.GreaterThan:
                    return await LeftOperand.GreaterThan(RightOperand)
                        ? Outcome.Success()
                        : Outcome.Fail("not greater");

                case ComparativeOperation.GreaterThanOrEquals:
                    return await LeftOperand.GreaterThanOrEquals(RightOperand)
                        ? Outcome.Success()
                        : Outcome.Fail("not greater or equal");

                case ComparativeOperation.Contains:
                    return await ((ScriptCollectionValue)LeftOperand).Contains(RightOperand);

                case ComparativeOperation.ContainsOrEqual:
                    var outcome = await ((ScriptCollectionValue)LeftOperand).Contains(RightOperand);
                    if (outcome)
                        return outcome;

                    return await LeftOperand.Equals(RightOperand)
                        ? Outcome<int>.Success(0)
                        : Outcome.Fail("not contains or equal");

                case ComparativeOperation.NotContains:
                    outcome = await ((ScriptCollectionValue)LeftOperand).Contains(RightOperand);
                    return outcome
                        ? Outcome<int>.Fail("contains")
                        : Outcome.Success();

                case ComparativeOperation.Contained:
                    return await ((ScriptCollectionValue)RightOperand).Contains(LeftOperand);

                case ComparativeOperation.ContainedOrEqual:
                    outcome = await ((ScriptCollectionValue)RightOperand).Contains(LeftOperand);
                    if (outcome)
                        return outcome;

                    return await LeftOperand.Equals(RightOperand)
                        ? Outcome<int>.Success(0)
                        : Outcome.Fail("not contained or equal");

                case ComparativeOperation.NotContained:
                    outcome = await ((ScriptCollectionValue)RightOperand).Contains(LeftOperand);
                    return outcome
                        ? Outcome<int>.Fail("contained")
                        : Outcome.Success();

                default:
                    return Errors.ExpectedComparativeOperator(StringValue);
            }
        }

        internal ScriptComparisonExpression(string stringValue, ScriptValue leftOperand, ScriptValue rightOperand,
            ComparativeOperation operation)
            : base(stringValue)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            Operation = operation;
        }
    }
}