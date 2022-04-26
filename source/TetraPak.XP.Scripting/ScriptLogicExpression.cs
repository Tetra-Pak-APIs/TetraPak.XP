using System.Threading.Tasks;

namespace TetraPak.XP.Scripting
{
    public sealed class ScriptLogicExpression : ScriptExpression
    {
        public ScriptExpression? Left { get; private set; }

        public ScriptExpression? Right { get; private set; }

        public ScriptLogicOperator Operator { get; private set; }

        internal bool IsComplete => Left is { } && Right is { };

        internal override ScriptExpression Invert()
        {
            var op = Operator.Invert();
            var left = Left!.Invert();
            var right = Right!.Invert();
            var stringValue = $"{left} {op.ToStringToken()} {right}";
            return new ScriptLogicExpression(stringValue).WithOperation(left, op, right);
        }

        public override async Task<Outcome> RunAsync(IScriptContext? context = null)
        {
            context ??= Context ?? ScriptContext.Default;
            var left = await Left!.RunAsync(context);
            if (left)
                return Operator == ScriptLogicOperator.Or
                    ? left
                    : await Right!.RunAsync(context);

            return Operator != ScriptLogicOperator.And
                ? left
                : await Right!.RunAsync(context);
        }

        internal ScriptLogicExpression WithOperation(ScriptExpression left, ScriptLogicOperator op,
            ScriptExpression right)
        {
            Left = left;
            Operator = op;
            Right = right;
            return this;
        }

        internal ScriptLogicExpression Expand(ScriptLogicOperator op, ScriptExpression right)
        {
            return new ScriptLogicExpression($"{StringValue} {op.ToStringToken()} {right}")
                .WithOperation(this, op, right);
        }

        public ScriptLogicExpression(string? stringValue)
            : base(stringValue)
        {
        }
    }
}