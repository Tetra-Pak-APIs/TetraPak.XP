using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using TetraPak.Serialization;

#nullable enable

namespace TetraPak
{
    /// <summary>
    ///   A specialized version of a <see cref="IStringValue"/> that supports multiple items.
    ///   This is useful for dealing with of textual representation of lists/collections etc.  
    /// </summary>
    /// <seealso cref="IStringValue"/>
    /// <seealso cref="StringValueBase"/>
    [Serializable, JsonConverter(typeof(JsonStringValueSerializer<MultiStringValue>))]
    [DebuggerDisplay("{" + nameof(StringValue) + "}")]
    public class MultiStringValue : StringValueBase, IEnumerable<string>
    {
        /// <summary>
        ///   The default separator used for parsing a <see cref="MultiStringValue"/>. 
        /// </summary>
        public const string DefaultSeparator = ",";

        /// <summary>
        ///   Gets a custom separator (initialized by ctor).
        /// </summary>
        public string Separator { get; protected set; }
        
        /// <summary>
        ///   Gets the string elements of the value as an <see cref="Array"/> of <see cref="string"/>.
        /// </summary>
        [JsonIgnore]
        public string[]? Items { get; protected set; }

        /// <summary>
        ///   Gets the number of <see cref="Items"/> in the value.
        /// </summary>
        public int Count => Items?.Length ?? 0;
        
        /// <summary>
        ///   Creates and returns an empty <see cref="MultiStringValue"/>.
        /// </summary>
        public static MultiStringValue Empty { get; } = new();

        public string this[int index] => Items?.Any() ?? false
            ? Items![index]
            : throw new ArgumentOutOfRangeException();

        /// <summary>
        ///   Called internally to resolve the item separator pattern in use.
        /// </summary>
        /// <returns></returns>
        protected virtual string OnGetSeparator() => Separator;

        /// <summary>
        ///   Converts a string to its <see cref="MultiStringValue"/> equivalent.
        ///   A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="stringValue">
        ///   A string containing a <see cref="MultiStringValue"/> to convert.
        /// </param>
        /// <param name="multiStringValue">
        ///   The successfully parsed <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="comparison">
        ///   (optional)<br/>
        ///   A <see cref="StringComparer"/> used for parsing the <see cref="MultiStringValue"/>.
        ///   This is mainly intended for the need in derived classes that needs to override the
        ///   <see cref="OnValidateItem"/> method. The comparer have no effect in this class. 
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="stringValue"/> was converted successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="TryParse{T}"/>
        public static bool TryParse(
            string stringValue,
            [NotNullWhen(true)] out MultiStringValue? multiStringValue,
            StringComparison comparison = StringComparison.Ordinal)
        {
            return TryParse<MultiStringValue>(stringValue, out multiStringValue, comparison);
        }
        
        /// <summary>
        ///   Converts a string to its <see cref="MultiStringValue"/>-compatible equivalent.
        ///   A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="stringValue"></param>
        /// <param name="multiStringValue"></param>
        /// <param name="comparison"></param>
        /// <typeparam name="T">
        ///   A <see cref="Type"/>, deriving from <see cref="MultiStringValue"/>,
        ///   for <paramref name="stringValue"/> to be converted to. 
        /// </typeparam>
        /// <returns>
        ///   <c>true</c> if <paramref name="stringValue"/> was converted successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="TryParse"/>
        public static bool TryParse<T>(
            string stringValue, 
            [NotNullWhen(true)] out T? multiStringValue, 
            StringComparison comparison = StringComparison.Ordinal)
        where T : MultiStringValue
        {
            multiStringValue = (T?) Activator.CreateInstance(typeof(T));
            if (multiStringValue is null)
                return false;
                
            var parseOutcome = multiStringValue.tryParse(stringValue, comparison);
            if (parseOutcome)
            {
                multiStringValue.setInternal(stringValue, parseOutcome.Value!);
                return true;
            }
            
            multiStringValue = null;
            return false;
        }

        void setInternal(string stringValue, string[] items)
        {
            StringValue = stringValue;
            Items = items;
        }

        protected void SetInternal(IEnumerable<string> items)
        {
            Items = items.ToArray();
            StringValue = Items.ConcatCollection(Separator);
        }

        Outcome<string[]> tryParse(string value, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(value))
                return Outcome<string[]>.Fail(new FormatException($"Invalid {typeof(MultiStringValue)} format: \"{value}\""));

            var separator = OnGetSeparator();
            var split = value.Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
                return Outcome<string[]>.Fail(new FormatException($"Invalid {typeof(MultiStringValue)} format: \"{value}\""));

            var roles = new List<string>();
            foreach (var s in split)
            {
                var trimmed = s.Trim();
                var isValidItem = OnValidateItem(trimmed, comparison);
                if (!isValidItem)
                    return Outcome<string[]>.Fail(isValidItem.Exception);
                
                roles.Add(trimmed);
            }
            return Outcome<string[]>.Success(roles.ToArray());
        }

        /// <summary>
        ///   Called during the parsing process to allow validation of a string item.
        ///   Intended for derived <see cref="MultiStringValue"/> classes. This implementation always returns
        ///   a successful result. 
        /// </summary>
        /// <param name="item">
        ///   A <see cref="string"/> item to be validated.
        /// </param>
        /// <param name="comparison">
        ///   (optional; default=<see cref="StringComparison.Ordinal"/>)<br/>
        ///   A string comparison value to be honored by the validation. 
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="string"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        protected virtual Outcome<string> OnValidateItem(string item, 
            StringComparison comparison = StringComparison.Ordinal) 
            => Outcome<string>.Success(item);

        /// <summary>
        ///   Implicitly converts a <see cref="MultiStringValue"/> value into its <see cref="string"/> representation.
        /// </summary>
        /// <param name="value">
        ///   A <see cref="MultiStringValue"/> value to be implicitly converted into its <see cref="string"/> representation.
        /// </param>
        /// <returns>
        ///   The <see cref="string"/> representation of <paramref name="value"/>.
        /// </returns>
        public static implicit operator string?(MultiStringValue? value) => value?.StringValue;
        
        /// <summary>
        ///   Implicit operator for parsing and casting a <see cref="string"/>
        ///   to a <see cref="MultiStringValue"/>.  
        /// </summary>
        /// <param name="stringValue">
        ///   
        /// </param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static implicit operator MultiStringValue(string? stringValue) => 
            stringValue is null 
                ? Empty 
                : new MultiStringValue(stringValue);

        /// <inheritdoc />
        public override string ToString() => StringValue!;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        public IEnumerator<string> GetEnumerator() => Items is null 
            ? new ArrayEnumerator<string>(Array.Empty<string>()) 
            : new ArrayEnumerator<string>(Items);

        #region .  Equality  .

        /// <summary>
        ///   Compares with another <see cref="MultiStringValue"/> and returns a value to indicate
        ///   whether all items in this value are present in the other, regardless of item's order.
        /// </summary>
        /// <param name="other">
        ///   The other <see cref="MultiStringValue"/> to compare with.
        /// </param>
        /// <param name="comparison">
        ///   (optional; default=<see cref="StringComparison.Ordinal"/>)<br/>
        ///   Specifies how to compare string values.
        /// </param>
        /// <returns>
        ///   <c>true</c> both values contains same number of <see cref="Items"/>
        ///   and all <see cref="Items"/> in this value are present in <paramref name="other"/>.
        /// </returns>
        public virtual bool EqualsSemantically(
            MultiStringValue? other, 
            StringComparison comparison = StringComparison.InvariantCulture)
        {
            var length = Items?.Length;
            if (other is null || length != other.Items?.Length)
                return false;

            for (var i = 0; i < length; i++)
            {
                var test = Items![i];
                var match = false;
                for (var j = 0; j < length && !match; j++)
                {
                    match = test.Equals(other.Items![j], comparison);
                }

                if (!match)
                    return false;
            }

            return true;
        }

        /// <summary>
        ///   Determines whether the specified value is equal to the current value.
        /// </summary>
        /// <param name="other">
        ///   A <see cref="MultiStringValue"/> value to compare to this value.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="other"/> is equal to the current value; otherwise <c>false</c>.
        /// </returns>
        public virtual bool Equals(MultiStringValue? other)
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
            return StringValue is not null ? StringValue.GetHashCode() : 0;
        }
        
        // public bool Contains(string value) => Items?.Contains(value) ?? false; obsolete

        /// <summary>
        ///   Comparison operator overload.
        /// </summary>
        public static bool operator ==(MultiStringValue? left, MultiStringValue? right)
        {
            return left?.Equals(right) ?? right is null;
        }

        /// <summary>
        ///   Comparison operator overload.
        /// </summary>
        public static bool operator !=(MultiStringValue? left, MultiStringValue? right)
        {
            return !left?.Equals(right) ?? right is not null;
        }

        /// <summary>
        ///   Overrides the <c>&gt;=</c> operator.
        /// </summary>
        /// <param name="left">
        ///   The left (<see cref="MultiStringValue"/>) operand.
        /// </param>
        /// <param name="right">
        ///   The right (<see cref="MultiStringValue"/>) operand.
        /// </param>
        /// <returns>
        ///   <c>true</c> if all <see cref="Items"/> of the <paramref name="right"/> <see cref="MultiStringValue"/>
        ///   can be found in the <paramref name="left"/> <see cref="MultiStringValue"/>.
        /// </returns>
        public static bool operator >=(MultiStringValue? left, MultiStringValue? right)
        {
            if (right?.Items is null || right.Items.Length == 0)
                return false;
                
            return right.Items.All(i => left?.Contains(i) ?? false);
        }

        /// <summary>
        ///   Overrides the <c>&lt;=</c> operator.
        /// </summary>
        /// <param name="left">
        ///   The left (<see cref="MultiStringValue"/>) operand.
        /// </param>
        /// <param name="right">
        ///   The right (<see cref="MultiStringValue"/>) operand.
        /// </param>
        /// <returns>
        ///   <c>true</c> if all <see cref="Items"/> of the <paramref name="left"/> <see cref="MultiStringValue"/>
        ///   can be found in the <paramref name="right"/> <see cref="MultiStringValue"/>.
        /// </returns>
        public static bool operator <=(MultiStringValue left, MultiStringValue right)
        {
            return right >= left;
        }
        #endregion
        
        void validateSupported(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                var isValid = OnValidateItem(item);
                if (!isValid)
                    throw isValid.Exception;
            }
        }

        /// <summary>
        ///   Creates a <see cref="MultiStringValue"/> from one or more <see cref="string"/> items,
        ///   automatically removing any duplicates.
        /// </summary>
        /// <param name="items">
        ///   The <see cref="string"/> items. 
        /// </param>
        /// <returns>
        ///   A <see cref="MultiStringValue"/>.
        /// </returns>
        public static MultiStringValue WithoutDuplicates(params string[] items)
        {
            var set = new HashSet<string>();
            foreach (var item in items)
            {
                if (set.Contains(item))
                    continue;
                
                set.Add(item);
            }
            return new MultiStringValue(set.ToArray());
        }

        void setStringValue() => StringValue = Items.ConcatCollection(OnGetSeparator());

        protected void SetStringValue(string stringValue) => StringValue = stringValue;

        protected void AddRange(IEnumerable<string> items)
        {
            var list = Items?.ToList() ?? new List<string>();
            list.AddRange(items);
            SetInternal(list);
        }

        protected void InsertRange(int index, IEnumerable<string> items)
        {
            var list = Items?.ToList() ?? new List<string>();
            list.InsertRange(index, items);
            SetInternal(list);
        }

        protected void RemoveAt(int index)
        {
            var list = Items?.ToList() ?? new List<string>();
            list.RemoveAt(index);
            SetInternal(list);
        }

        /// <summary>
        ///   Initializes an <see cref="Empty"/> <see cref="MultiStringValue"/>.
        /// </summary>
        public MultiStringValue() : base("")
        {
            Separator = DefaultSeparator;
        }

        /// <summary>
        ///   Initializes the value.
        /// </summary>
        /// <param name="stringValue">
        ///   The new value's string representation (will automatically be parsed).
        /// </param>
        /// <param name="separator">
        ///   (optional; default=<see cref="DefaultSeparator"/>)<br/>
        ///   A custom separator.
        /// </param>
        /// <exception cref="FormatException">
        ///   The <paramref name="stringValue"/> string representation was incorrectly formed.
        /// </exception>
        /// <seealso cref="DefaultSeparator"/>
        //[DebuggerStepThrough] 
        public MultiStringValue(string? stringValue, string? separator = null) 
        : base(stringValue)
        {
            Separator = string.IsNullOrEmpty(separator) ? DefaultSeparator : separator;
            if (IsError || string.IsNullOrWhiteSpace(stringValue))
                return;
            
            var parseOutcome = tryParse(stringValue);
            if (!parseOutcome)
                throw parseOutcome.Exception;

            Items = parseOutcome.Value;
            SetStringValue(stringValue);
        }

        /// <summary>
        ///   Initializes an <see cref="MultiStringValue"/> from an array of <see cref="string"/> items.
        /// </summary>
        /// <param name="items">
        ///   Initializes <see cref="Items"/>.
        /// </param>
        /// <param name="separator">
        ///   (optional; default=<see cref="DefaultSeparator"/>)<br/>
        ///   Initializes <see cref="Separator"/>.
        /// </param>
        public MultiStringValue(string[] items, string? separator = null) 
        : base("")
        {
            Separator = string.IsNullOrEmpty(separator) ? DefaultSeparator : separator;
            if (items.Length == 0)
                return;

            validateSupported(items);
            Items = items;
            setStringValue();
        }
    }
}