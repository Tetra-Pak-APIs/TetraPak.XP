using System;

namespace TetraPak.XP.Scripting
{
    public class ScriptLogicExpression : ScriptExpression
    {
        public ScriptExpression? Left { get; private set; }

        public ScriptExpression? Right { get; private set; }

        public ScriptLogicOperator Operator { get; private set; }
        
        internal bool IsComplete => Left is { } && Right is { };

        internal override ScriptExpression Invert()
        {
            var op = Operator.Invert();
            var left =  Left!.Invert();
            var right = Right!.Invert();
            var stringValue = $"{left} {op.ToStringToken()} {right}";
            return new ScriptLogicExpression(stringValue).WithOperation(left, op, right);
        }

        public override bool IsMatch(StringComparison comparison)
        {
            throw new NotImplementedException();
        }

        // public override bool IsMatch(HttpRequest request, StringComparison comparison = StringComparison.InvariantCulture)
        // {
        //     var left = request.IsMatch(Left!);
        //     if (left)
        //         return Operator == ScriptLogicOperator.Or || request.IsMatch(Right!);
        //
        //     return Operator != ScriptLogicOperator.And && request.IsMatch(Right!);
        // }

        internal ScriptLogicExpression WithOperation(ScriptExpression left, ScriptLogicOperator op, ScriptExpression right)
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