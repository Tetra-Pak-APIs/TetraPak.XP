using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using TetraPak.XP.Serialization;

namespace TetraPak.XP.StringValues
{
    /// <summary>
    ///   A basic implementation of <see cref="IStringValue"/>.
    /// </summary>
    /// <remarks>
    ///   -- TODO -- Write XML documentation explaining the concept (and benefits) of string values  
    /// </remarks>
    /// <seealso cref="IStringValue"/>
    /// <seealso cref="MultiStringValue"/>
    [Serializable, JsonConverter(typeof(JsonStringValueSerializer<StringValueBase>))]
    [DebuggerDisplay("{ToString()}")]
    public class StringValueBase : IStringValue // todo consider making StringValueBase immutable
    {
        protected const string NoParse = "__(NOPARSE)__";

        readonly int _hashCode;

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public bool IsError { get; set; }

        /// <summary>
        ///   The textual representation of the <see cref="IStringValue"/>.
        /// </summary>
        public string StringValue { get; }

        protected StringValueParseResult Parse(string? stringValue) => OnParse(stringValue);

        /// <summary>
        ///   Instantiates a <see cref="IStringValue"/> of the specified type.
        /// </summary>
        /// <param name="s">
        ///   The textual representation of the requested <see cref="IStringValue"/>.
        /// </param>
        /// <typeparam name="T">
        ///   The type of <see cref="IStringValue"/> to be constructed.   
        /// </typeparam>
        /// <returns>
        ///   A <see cref="IStringValue"/> object of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="TargetInvocationException">
        ///   The <typeparamref name="T"/> type does not implement <see cref="IStringValue"/>.
        /// </exception>
        public static T MakeStringValue<T>(string? s) => (T)MakeStringValue(typeof(T), s);

        /// <summary>
        ///   Instantiates a <see cref="IStringValue"/> of the specified type.
        /// </summary>
        /// <param name="targetType">
        ///   The type of <see cref="IStringValue"/> to be constructed.
        /// </param>
        /// <param name="s">
        ///   The textual representation of the requested <see cref="IStringValue"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="IStringValue"/> object of type <paramref name="targetType"/>.
        /// </returns>
        /// <exception cref="TargetInvocationException">
        ///   The <paramref name="targetType"/> type does not implement <see cref="IStringValue"/>.
        /// </exception>
        public static object MakeStringValue(Type targetType, string? s)
        {
            if (s is null)
                return default!;

            if (!typeof(IStringValue).IsAssignableFrom(targetType))
                throw new TargetInvocationException(
                    $"Cannot make string value of type {targetType}. Not a {typeof(IStringValue)}", null);

            var ctor = targetType.GetConstructor(new[] { typeof(string) });
            if (ctor is null)
                throw new TargetInvocationException(
                    $"Cannot make {targetType}. Expected ctor with single string parameter", null);

            return ctor.Invoke(new object[] { s });
        }

        /// <summary>
        ///   To be overridden.
        ///   Invoked from the ctor (<see cref="StringValueBase(string)"/>) to automatically parse the
        ///   passed <see cref="string"/> representation of the value.
        /// </summary>
        /// <param name="stringValue">
        ///   The <see cref="string"/> representation to be parsed (by an overriding method).
        /// </param>
        /// <returns>
        ///   The (possibly transformed) <paramref name="stringValue"/>.
        /// </returns>
        /// <remarks>
        ///   This base implementation will not actually parse the <paramref name="stringValue"/>.
        ///   Instead it will simply look for the <see cref="Identifiers.ErrorQualifier"/> to determine whether
        ///   it is already an erroneous <see cref="IStringValue"/>.  
        /// </remarks>
        protected virtual StringValueParseResult OnParse(string? stringValue)
        {
            if (stringValue.IsUnassigned(true) || !stringValue!.StartsWith(Identifiers.ErrorQualifier))
                return new StringValueParseResult(stringValue ?? string.Empty, stringValue?.GetHashCode() ?? 0);

            IsError = true;
            return new StringValueParseResult(stringValue, stringValue.GetHashCode());
        }

        protected bool IsAssignedAndNotError(string? stringValue)
            => stringValue.IsAssigned(true) && !stringValue!.StartsWith(Identifiers.ErrorQualifier);

        /// <inheritdoc />
        public override string ToString() => StringValue;

        #region .  Equality  .

        /// <summary>
        ///   Determines whether the specified value is equal to the current value.
        /// </summary>
        /// <param name="other">
        ///   A <see cref="StringValueBase"/> value to compare to this value.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="other"/> is equal to the current value; otherwise <c>false</c>.
        /// </returns>
        public bool Equals(StringValueBase? other) => other is not null && _hashCode == other._hashCode;

        /// <summary>
        ///   Determines whether the specified object is equal to the current version.
        /// </summary>
        /// <param name="obj">
        ///   An object to compare to this value.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the specified object is equal to the current version; otherwise <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is MultiStringValue other && Equals(other);

        /// <summary>
        ///   Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///   A hash code for the current value.
        /// </returns>
        public override int GetHashCode() => _hashCode;

        /// <summary>
        ///   Comparison operator overload.
        /// </summary>
        public static bool operator ==(StringValueBase? left, StringValueBase? right)
        {
            return left?.Equals(right) ?? right is null;
        }

        /// <summary>
        ///   Comparison operator overload.
        /// </summary>
        public static bool operator !=(StringValueBase? left, StringValueBase? right)
        {
            return !left?.Equals(right) ?? right is not null;
        }

        #endregion

        protected string AsError(string? stringValue) => $"{Identifiers.ErrorQualifier}{stringValue ?? ""}";

        protected string AsParsed(string value) => $"{NoParse}{value}";

        static bool isParsed(string? stringValue) => stringValue.IsAssigned() && stringValue!.StartsWith(NoParse);

        static string stripParsed(string stringValue) => stringValue.Substring(NoParse.Length);

        public static bool TryConstruct(Type stringValueType, string s, out IStringValue? stringValue)
        {
            if (!stringValueType.IsImplementingInterface<IStringValue>())
                throw new ArgumentException($"Type is not a {typeof(IStringValue)}: {stringValueType}");

            ConstructorInfo? ctor = null;
            var constructors = stringValueType.GetConstructors();
            foreach (var c in constructors)
            {
                var infos = c.GetParameters();
                if (infos.Length == 0)
                    continue;

                if (infos[0].ParameterType != typeof(string))
                    continue;

                if (!isSingleParameterOrRestAreOptional(infos))
                    continue;

                ctor = c;
            }

            if (ctor is null)
            {
                stringValue = null;
                return false;
            }

            stringValue = (IStringValue)Activator.CreateInstance(stringValueType, s);
            return true;

            bool isSingleParameterOrRestAreOptional(IReadOnlyList<ParameterInfo> parameterInfos)
            {
                if (parameterInfos.Count == 1)
                    return true;

                for (var i = 1; i < parameterInfos.Count; i++)
                {
                    if (!parameterInfos[i].IsOptional)
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        ///   Initializes the <see cref="IStringValue"/>.
        /// </summary>
        /// <param name="stringValue">
        ///   The textual representation of the <see cref="IStringValue"/>.
        /// </param>
        public StringValueBase(string? stringValue)
        {
            if (isParsed(stringValue))
            {
                StringValue = stripParsed(stringValue!);
                _hashCode = StringValue.GetHashCode();
                return;
            }

            var parseResult = Parse(stringValue);
            StringValue = parseResult.StringValue;
            _hashCode = parseResult.HashCode;
        }
    }

    public sealed class StringValueParseResult
    {
        public string StringValue { get; set; }

        public int HashCode { get; }

        public static StringValueParseResult Empty => new(string.Empty, string.Empty.GetHashCode());

        public StringValueParseResult(string stringValue, int hashCode)
        {
            StringValue = stringValue;
            HashCode = hashCode;
        }
    }
}