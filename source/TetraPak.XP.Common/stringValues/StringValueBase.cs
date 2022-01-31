using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;

#nullable enable

namespace TetraPak.XP
{
    /// <summary>
    ///   A basic implementation of <see cref="IStringValue"/>.
    /// </summary>
    /// <remarks>
    ///   -- TODO -- Write XML documentation explaining the concept (and benefits) of string values  
    /// </remarks>
    /// <seealso cref="IStringValue"/>
    /// <seealso cref="MultiStringValue"/>
    // [Serializable, JsonConverter(typeof(JsonStringValueSerializer<StringValueBase>))]
    [DebuggerDisplay("ToString()")]
    public class StringValueBase : IStringValue // todo consider making StringValueBase immutable
    {
        string? _stringValue;

        /// <summary>
        ///   A string identifier to qualify an erroneous string value. 
        /// </summary>
        public const string ErrorQualifier = "#ERROR:";
        
        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public bool IsError { get; set; }

        /// <summary>
        ///   The textual representation of the <see cref="IStringValue"/>.
        /// </summary>
        public virtual string StringValue 
        {
            get => _stringValue ?? string.Empty;
            protected set => _stringValue = value;
        } 

        string? parse(string? stringValue) => OnParse(stringValue);

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
        ///   A <see cref="IStringValue"/> object of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="TargetInvocationException">
        ///   The <typeparamref name="T"/> type does not implement <see cref="IStringValue"/>.
        /// </exception>
        public static object MakeStringValue(Type targetType, string? s)
        {
            if (s is null)
                return default!;
                
            if (!typeof(IStringValue).IsAssignableFrom(targetType))
                throw new TargetInvocationException(
                    $"Cannot make string value of type {targetType}. Not a {typeof(IStringValue)}", null);
                
            var ctor = targetType.GetConstructor(new[] {typeof(string)});
            if (ctor is null)
                throw new TargetInvocationException(
                    $"Cannot make {targetType}. Expected ctor with single string parameter", null);

            return ctor.Invoke(new object[] {s});
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
        ///   Instead it will simply look for the <see cref="ErrorQualifier"/> to determine whether
        ///   it is already an erroneous <see cref="IStringValue"/>.  
        /// </remarks>
        protected virtual string? OnParse(string? stringValue)
        {
            if (!stringValue.IsAssigned(true) || !stringValue!.StartsWith(ErrorQualifier))
                return stringValue;

            IsError = true;
            return stringValue;
        }

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
        public bool Equals(StringValueBase? other)
        {
            return other is not null && string.Equals(StringValue, other.StringValue);
        }

        /// <summary>
        ///   Determines whether the specified object is equal to the current version.
        /// </summary>
        /// <param name="obj">
        ///   An object to compare to this value.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the specified object is equal to the current version; otherwise <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return obj is MultiStringValue other && Equals(other);
        }

        /// <summary>
        ///   Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///   A hash code for the current value.
        /// </returns>
        public override int GetHashCode()
        {
            return StringValue is {} ? StringValue.GetHashCode() : 0;
        }
        
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
        
        void setStringValue(string stringValue)
        {
            StringValue = stringValue;
        }

        /// <summary>
        ///   Initializes the <see cref="IStringValue"/>.
        /// </summary>
        /// <param name="stringValue">
        ///   The textual representation of the <see cref="IStringValue"/>.
        /// </param>
#pragma warning disable CS8618
        public StringValueBase(string? stringValue)
        {
            setStringValue(parse(stringValue) ?? string.Empty);
        }
#pragma warning restore CS8618
    }
}