using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TetraPak.XP.Scripting
{
    [DebuggerDisplay("{ToString()}")]
    sealed class NumericLiteralValue : ScriptValue
    {
        public override string ValueTypeIdentifier => IsInteger ? "integer" : "decimal";
        
        public override Task<bool> IsValidOperationAsync(ComparativeOperation operation)
        {
            return Task.FromResult(operation switch
            {
                ComparativeOperation.None => false,
                ComparativeOperation.Equal => true,
                ComparativeOperation.NotEqual => true,
                ComparativeOperation.LessThan => true,
                ComparativeOperation.LessThanOrEquals => true,
                ComparativeOperation.GreaterThan => true,
                ComparativeOperation.GreaterThanOrEquals => true,
                ComparativeOperation.Contains => false,
                ComparativeOperation.ContainsOrEqual => false,
                ComparativeOperation.NotContains => false,
                ComparativeOperation.Contained => true,
                ComparativeOperation.NotContained => true,
                ComparativeOperation.ContainedOrEqual => true,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            });
        }
        
        public double Value { get; }

        public override string ToString() => StringValue;

        public bool IsInteger => Value % 1 == 0; 

        public override Task<Outcome> Equals(ScriptValue value)
        {
            if (value is NullLiteralValue)
                return Task.FromResult(Outcome.Fail($"{this} is not equal to {value}"));

            if (value is NumericLiteralValue nv)
                return Task.FromResult(Math.Abs(Value - nv.Value) < double.Epsilon
                    ? Outcome.Success() 
                    : Outcome.Fail($"{this} is not equal to {value}"));
            
            if (value.TryCastTo<double>(out var dValue))
                return Task.FromResult(Math.Abs(Value - dValue) < double.Epsilon
                    ? Outcome.Success() 
                    : Outcome.Fail($"{this} is not equal to {value}"));
                    
            return Task.FromResult(Outcome.Fail($"{this} is not equal to {value}"));
        }
        
        public override async Task<Outcome> NotEquals(ScriptValue value)
        {
            var outcome = await Equals(value);
            return outcome
                ? Outcome.Fail($"{this} is equal to {value}")
                : Outcome.Success();
        }

        public override Task<Outcome> LessThan(ScriptValue value)
        {
            if (value is NullLiteralValue)
                return Task.FromResult(Outcome.Fail($"{this} is not less than {value}"));

            if (value is NumericLiteralValue nv)
                return Task.FromResult(Value < nv.Value
                    ? Outcome.Success() 
                    : Outcome.Fail($"{this} is not less than {value}"));
            
            if (value.TryCastTo<double>(out var dValue))
                return Task.FromResult(Value < dValue
                    ? Outcome.Success() 
                    : Outcome.Fail($"{this} is not less than {value}"));
            
            return Task.FromResult(Outcome.Fail($"{this} is not less than {value}"));
        }

        public override Task<Outcome> LessThanOrEquals(ScriptValue value)
        {
            if (value is NullLiteralValue)
                return Task.FromResult(Outcome.Fail($"{this} is not less than or equal to {value}"));

            if (value is NumericLiteralValue nv)
                return Task.FromResult(Value <= nv.Value
                    ? Outcome.Success() 
                    : Outcome.Fail($"{this} is not less than or equal to {value}"));

            if (value.TryCastTo<double>(out var dValue))
                return Task.FromResult(Value <= dValue
                    ? Outcome.Success() 
                    : Outcome.Fail($"{this} is not less than or equal to {value}"));

            return Task.FromResult(Outcome.Fail($"{this} is not less than or equal to {value}"));
        }

        public override Task<Outcome> GreaterThan(ScriptValue value)
        {
            if (value is NullLiteralValue)
                return Task.FromResult(Outcome.Fail($"{this} is not greater than {value}"));

            if (value is NumericLiteralValue nv)
                return Task.FromResult(Value > nv.Value
                    ? Outcome.Success() 
                    : Outcome.Fail($"{this} is not greater than {value}"));

            if (value.TryCastTo<double>(out var dValue))
                return Task.FromResult(Value > dValue
                    ? Outcome.Success() 
                    : Outcome.Fail($"{this} is not greater than {value}"));

            return Task.FromResult(Outcome.Fail($"{this} is not greater than {value}"));
        }

        public override Task<Outcome> GreaterThanOrEquals(ScriptValue value)
        {
            if (value is NullLiteralValue)
                return Task.FromResult(Outcome.Fail($"{this} is not greater than or equal to {value}"));

            if (value is NumericLiteralValue nv)
                return Task.FromResult(Value >= nv.Value
                    ? Outcome.Success() 
                    : Outcome.Fail($"{this} is not greater than or equal to {value}"));

            if (value.TryCastTo<double>(out var dValue))
                return Task.FromResult(Value >= dValue
                    ? Outcome.Success() 
                    : Outcome.Fail($"{this} is not greater than or equal to {value}"));

            return Task.FromResult(Outcome.Fail($"{this} is not greater than or equal to {value}"));
        }

        public override bool TryCastTo<T>(out T value)
        {
            if (typeof(T) == typeof(string))
            {
                value = (T) (object) ToString()!; // todo consider using standard format for string representation of numeric literal
                return true;
            }

            if (typeof(T).IsNumeric())
            {
                var obj = Convert.ChangeType(Value, typeof(T));
                if (obj is T tValue)
                {
                    value = tValue;
                    return true;
                }
            }

            value = default!;
            return false;
        }

        public NumericLiteralValue(string stringValue, double value) 
        : base(stringValue)
        {
            Value = value;
        }
    }
}