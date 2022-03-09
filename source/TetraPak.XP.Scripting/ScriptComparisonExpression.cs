using System;

namespace TetraPak.XP.Scripting
{
    /// <summary>
    ///   A string compatible (criteria) expression for use with HTTP requests.
    /// </summary>
    public class ScriptComparisonExpression : ScriptExpression
    {
        /// <summary>
        ///   Specifies the comparative operator.
        /// </summary>
        /// <seealso cref="ComparativeOperation"/>
        public ComparativeOperation Operator { get; private set; }

        public ScriptValue LeftOperand { get; }

        public ScriptValue RightOperand { get; }

        internal override ScriptExpression Invert()
        {
            var op = Operator.Invert();
            var stringValue = $"{LeftOperand}] {op.ToStringToken()} {RightOperand}";
            return new ScriptComparisonExpression(stringValue, LeftOperand, RightOperand);
        }


        public override bool IsMatch(StringComparison comparison)
        {
            return Operator switch
            {
                ComparativeOperation.Equal => LeftOperand.Equals(RightOperand, comparison),
                ComparativeOperation.NotEqual => !LeftOperand.Equals(RightOperand, comparison),
                ComparativeOperation.LessThan => !LeftOperand.LessThan(RightOperand, comparison),
                ComparativeOperation.LessThanOrEquals => !LeftOperand.LessThanOrEquals(RightOperand, comparison),
                ComparativeOperation.GreaterThan => !LeftOperand.GreaterThan(RightOperand, comparison),
                ComparativeOperation.GreaterThanOrEquals => !LeftOperand.GreaterThanOrEquals(RightOperand, comparison),
                ComparativeOperation.Contains => LeftOperand.Contains(RightOperand, comparison),
                ComparativeOperation.NotContains => !LeftOperand.Contains(RightOperand!, comparison),
                _ => false
            };
        }

        internal ScriptComparisonExpression(string stringValue, ScriptValue leftOperand, ScriptValue rightOperand)
        : base(stringValue)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }
    }
}