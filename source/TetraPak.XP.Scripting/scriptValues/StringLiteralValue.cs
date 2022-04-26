using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Scripting
{
    [DebuggerDisplay("{ToString()}")]
    sealed class StringLiteralValue : ScriptCollectionValue
    {
        public override string ValueTypeIdentifier => "string";

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
                ComparativeOperation.Contains => true,
                ComparativeOperation.ContainsOrEqual => true,
                ComparativeOperation.NotContains => true,
                ComparativeOperation.Contained => true,
                ComparativeOperation.NotContained => true,
                ComparativeOperation.ContainedOrEqual => true,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            });
        }

        public bool IsLiteral { get; }

        public override string ToString()
        {
            return IsLiteral
                ? $"{ScriptTokens.StringLiteralQualifier}{StringValue}{ScriptTokens.StringLiteralQualifier}"
                : StringValue;
        }

        public override Task<Outcome> Equals(ScriptValue value)
        {
            if (value is StringLiteralValue sv)
                return Task.FromResult(string.Equals(StringValue, sv.StringValue, StringComparison)
                    ? Outcome.Success()
                    : Outcome.Fail($"{this} is not equal to {value}"));

            if (value is NullLiteralValue)
                return Task.FromResult(Outcome.Fail($"{this} is not equal to {value}"));

            return Task.FromResult(value.TryCastTo<string>(out var s) && string.Equals(StringValue, s, StringComparison)
                ? Outcome.Success()
                : Outcome.Fail($"{this} is not equal to {value}"));
        }

        public override async Task<Outcome> NotEquals(ScriptValue value) // => !Equals(value); 
        {
            var outcome = await Equals(value);
            return outcome
                ? Outcome.Fail($"{this} is equal to {value}")
                : Outcome.Success();
        }

        public override Task<Outcome<int>> Contains(ScriptValue value)
        {
            if (value is NullLiteralValue)
                return Task.FromResult(Outcome<int>.Fail("not contains"));

            int idx;
            if (value is StringLiteralValue sv)
            {
                idx = StringValue.IndexOf(sv.StringValue, StringComparison);
                return Task.FromResult(idx >= 0
                    ? Outcome<int>.Success(idx)
                    : Outcome<int>.Fail("not contains"));
            }

            if (!value.TryCastTo<string>(out var s))
                return Task.FromResult(Outcome<int>.Fail("not contains"));

            idx = StringValue.IndexOf(s, StringComparison);
            return Task.FromResult(idx >= 0
                ? Outcome<int>.Success(idx)
                : Outcome<int>.Fail("not contains"));
        }

        public override async Task<Outcome> NotContains(ScriptValue value) // => !Contains(value);
        {
            var outcome = await Contains(value);
            return outcome
                ? Outcome.Fail($"{this} contains {value}")
                : Outcome.Success();
        }

        public override Task<Outcome> LessThan(ScriptValue value)
        {
            if (value is NullLiteralValue)
                return Task.FromResult(Outcome.Fail($"{this} is not equal to {value}"));

            if (value is StringLiteralValue sv)
                return Task.FromResult(string.Compare(StringValue, sv.StringValue, StringComparison) < 0
                    ? Outcome.Success()
                    : Outcome.Fail($"{this} is not less than {value}"));

            return Task.FromResult(
                value.TryCastTo<string>(out var s) && string.Compare(StringValue, s, StringComparison) < 0
                    ? Outcome.Success()
                    : Outcome.Fail($"{this} is not less than {value}"));
        }

        public override Task<Outcome> LessThanOrEquals(ScriptValue value)
        {
            if (value is NullLiteralValue)
                return Task.FromResult(Outcome.Fail($"{this} is not less than or equal to {value}"));

            if (value is StringLiteralValue sv)
                return Task.FromResult(string.Compare(StringValue, sv.StringValue, StringComparison) <= 0
                    ? Outcome.Success()
                    : Outcome.Fail($"{this} is not less than or equal to {value}"));


            return Task.FromResult(
                value.TryCastTo<string>(out var s) && string.Compare(StringValue, s, StringComparison) <= 0
                    ? Outcome.Success()
                    : Outcome.Fail($"{this} is not less than or equal to {value}"));
        }

        public override Task<Outcome> GreaterThan(ScriptValue value)
        {
            if (value is NullLiteralValue)
                return Task.FromResult(Outcome.Fail($"{this} is not greater than {value}"));

            if (value is StringLiteralValue sv)
                return Task.FromResult(string.Compare(StringValue, sv.StringValue, StringComparison) > 0
                    ? Outcome.Success()
                    : Outcome.Fail($"{this} is not greater than {value}"));

            return Task.FromResult(
                value.TryCastTo<string>(out var s) && string.Compare(StringValue, s, StringComparison) > 0
                    ? Outcome.Success()
                    : Outcome.Fail($"{this} is not greater than {value}"));
        }

        public override Task<Outcome> GreaterThanOrEquals(ScriptValue value)
        {
            if (value is NullLiteralValue)
                return Task.FromResult(Outcome.Fail($"{this} is not greater than or equal to {value}"));

            if (value is StringLiteralValue sv)
                return Task.FromResult(string.Compare(StringValue, sv.StringValue, StringComparison) >= 0
                    ? Outcome.Success()
                    : Outcome.Fail($"{this} is not greater than or equal to {value}"));

            return Task.FromResult(
                value.TryCastTo<string>(out var s) && string.Compare(StringValue, s, StringComparison) >= 0
                    ? Outcome.Success()
                    : Outcome.Fail($"{this} is not greater than or equal to {value}"));
        }

        public override bool TryCastTo<T>(out T? value) where T : default
        {
            if (typeof(T) == typeof(string))
            {
                value = (T)(object)StringValue;
                return true;
            }

            if (typeof(T).IsImplementingInterface<IStringValue>() &&
                StringValueBase.TryConstruct(typeof(T), StringValue, out var stringValueObj))
            {
                value = stringValueObj is T tv ? tv : default;
                return value != null;
            }

            value = default;
            return false;
        }

        public StringLiteralValue(string value)
            : base(trimStringValue(value, out var isStringLiteral))
        {
            IsLiteral = isStringLiteral;
        }


        static string trimStringValue(string value, out bool isStringLiteral)
        {
            isStringLiteral = value.StartsWith(ScriptTokens.StringLiteralQualifier) &&
                              value.EndsWith(ScriptTokens.StringLiteralQualifier);
            return isStringLiteral
                ? value.TrimPrefix(ScriptTokens.StringLiteralQualifier).TrimPostfix(ScriptTokens.StringLiteralQualifier)
                : value;
        }
    }
}