using System;

namespace TetraPak.XP.Scripting
{
    class StringLiteral : ScriptValue
    {
        public override bool IsCollection => true;

        public override bool Equals(ScriptValue value, StringComparison comparison)
        {
            throw new NotImplementedException();
        }

        public override bool NotEquals(ScriptValue value, StringComparison comparison)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(ScriptValue value, StringComparison comparison)
        {
            throw new NotImplementedException();
        }

        public override bool NotContains(ScriptValue value, StringComparison comparison)
        {
            throw new NotImplementedException();
        }

        public override bool Contained(ScriptValue value, StringComparison comparison)
        {
            throw new NotImplementedException();
        }

        public override bool NotContained(ScriptValue value, StringComparison comparison)
        {
            throw new NotImplementedException();
        }

        public override bool LessThan(ScriptValue value, StringComparison comparison)
        {
            throw new NotImplementedException();
        }

        public override bool LessThanOrEquals(ScriptValue value, StringComparison comparison)
        {
            throw new NotImplementedException();
        }

        public override bool GreaterThan(ScriptValue value, StringComparison comparison)
        {
            throw new NotImplementedException();
        }

        public override bool GreaterThanOrEquals(ScriptValue value, StringComparison comparison)
        {
            throw new NotImplementedException();
        }
        
        public StringLiteral(string value) : base(value)
        {
        }
    }
}