using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using TetraPak.XP.Serialization;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace TetraPak.XP.StringValues
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
        // these thread-statics are always initialized from `WithArgs`, which is always invoked from ctor
        [ThreadStatic]
        static string? s_separator;
        [ThreadStatic]
        static StringComparison? s_comparison;

        /// <summary>
        ///   The default separator used for parsing a <see cref="MultiStringValue"/>. 
        /// </summary>
        public const string DefaultSeparator = ",";

        /// <summary>
        ///   Gets a custom separator (initialized by ctor).
        /// </summary>
        public string Separator { get; protected set; }

        public StringComparison Comparison { get; private set; }

        /// <summary>
        ///   Gets the string elements of the value as an <see cref="Array"/> of <see cref="string"/>.
        /// </summary>
        [JsonIgnore]
        public string[] Items { get; protected set; }
        
        [JsonIgnore]
        public bool IsEmpty { get; protected set; }

        /// <summary>
        ///   Constructs and returns an empty <see cref="MultiStringValue"/>.
        /// </summary>
        public static MultiStringValue Empty => new();

        /// <summary>
        ///   Gets the number of <see cref="Items"/> in the value.
        /// </summary>
        public int Count => Items.Length;

        public string this[int index] => Items.Any()
            ? Items[index]
            : throw new ArgumentOutOfRangeException();

        /// <summary>
        ///   Invoked when parsing is done and needs to validate the items to be assigned to
        ///   the <see cref="Items"/> property. This provides an opportunity for derived classes
        ///   to perform validation
        /// </summary>
        /// <param name="items">
        ///   Ar array of items to be validated.
        /// </param>
        /// <returns></returns>
        protected virtual Outcome<string[]> OnValidate(string[] items)
        {
            for (var i = 0; i < items.Length; i++)
            {
                var outcome = OnValidateItem(items[i]);
                if (!outcome)
                    return Outcome<string[]>.Fail(outcome.Exception!);

                items[i] = outcome.Value!;
            }

            return Outcome<string[]>.Success(items);
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
        protected virtual Outcome<string> OnValidateItem(
            string item, 
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
        // [DebuggerStepThrough]
        public static implicit operator MultiStringValue(string? stringValue) => new(stringValue, DefaultSeparator);

        /// <inheritdoc />
        public override string ToString() => StringValue;

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        public IEnumerator<string> GetEnumerator() => new ArrayEnumerator<string>(Items);

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
            StringComparison? comparison = null)
        {
            var length = Items.Length;
            if (other is null || length != other.Items.Length)
                return false;

            var useComparison = comparison ?? Comparison;
            for (var i = 0; i < length; i++)
            {
                var test = Items[i];
                var match = false;
                for (var j = 0; j < length && !match; j++)
                {
                    match = test.Equals(other.Items[j], useComparison);
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
            return other is not null && string.Equals(StringValue, other.StringValue, Comparison);
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
        public override int GetHashCode() => StringValue.GetHashCode();

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
        
        string[] validate(string[] items)
        {
            var outcome = OnValidate(items);
            if (!outcome)
                throw outcome.Exception!;
                
            return outcome.Value!;
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

        /// <summary>
        ///   Clones the <see cref="Items"/> value and returns it with one or more items
        ///   appended to the end.
        /// </summary>
        /// <param name="items">
        ///   One or more items to be added. 
        /// </param>
        /// <returns>
        ///   An array of <see cref="string"/>s.
        /// </returns>
        protected string[] AddRange(IEnumerable<string> items) => AddRange(items.ToArray());

        /// <summary>
        ///   Clones the <see cref="Items"/> value and returns it with one or more items
        ///   appended to the end.
        /// </summary>
        /// <param name="items">
        ///   One or more items to be added. 
        /// </param>
        /// <returns>
        ///   An array of <see cref="string"/>s.
        /// </returns>
        protected string[] AddRange(params string[] items)
        {
            var list = Items.ToList();
            list.AddRange(items);
            return list.ToArray();
        }

        /// <summary>
        ///   Clones the <see cref="Items"/> value and returns it with one or more items
        ///   inserted at a specified position.
        /// </summary>
        /// <param name="index">
        ///   Specifies the position where the <paramref name="items"/> should be inserted.
        /// </param>
        /// <param name="items">
        ///   One or more items to be inserted. 
        /// </param>
        /// <returns>
        ///   An array of <see cref="string"/>s.
        /// </returns>
        protected string[] InsertRange(int index, IEnumerable<string> items) => InsertRange(index, items.ToArray());

        /// <summary>
        ///   Clones the <see cref="Items"/> value and returns it with one or more items
        ///   inserted at a specified position.
        /// </summary>
        /// <param name="index">
        ///   Specifies the position where the <paramref name="items"/> should be inserted.
        /// </param>
        /// <param name="items">
        ///   One or more items to be inserted. 
        /// </param>
        /// <returns>
        ///   An array of <see cref="string"/>s.
        /// </returns>
        protected string[] InsertRange(int index, params string[] items)
        {
            var list = Items.ToList();
            list.InsertRange(index, items);
            return list.ToArray();
        }
        
        /// <summary>
        ///   Clones the <see cref="Items"/> value and returns it with one or more items
        ///   removed from specified position.
        /// </summary>
        /// <param name="index">
        ///   Specifies the position where items should be removed.
        /// </param>
        /// <param name="count">
        ///   (optional; default = 1, min value = 1)<br/>
        ///   The number of items to be removed from the result.
        /// </param>
        /// <returns>
        ///   An array of <see cref="string"/>s.
        /// </returns>
        protected string[] RemoveAt(int index, int count = 1)
        {
            count = Math.Max(count, 1);
            var list = Items.ToList();
            for (var i = 0; i < count; i++)
            {
                list.RemoveAt(index);
            }
            return list.ToArray();
        }
        
        /// <summary>
        ///   (fluent api; intended for use with ctor)<br/>
        ///   Assigns the <see cref="Separator"/> and <see cref="Comparison"/> properties and returns the
        ///   passed in <paramref name="stringValue"/>.
        /// </summary>
        protected static string? WithArgs(string? stringValue, string? separator, StringComparison comparison)
        {
            s_separator = separator?.Trim() ?? separator ?? DefaultSeparator;
            s_comparison = comparison;
            return stringValue;
        }

        /// <summary>
        ///   Overrides the base method to support initialization and validation of <see cref="Items"/>.
        /// </summary>
        /// <param name="stringValue"></param>
        /// <returns>
        ///   A (possibly coerced) value to be assigned as <see cref="StringValueBase.StringValue"/>.
        /// </returns>
        /// <seealso cref="OnValidate"/>
        /// <seealso cref="OnValidateItem"/>
        protected override StringValueParseResult OnParse(string? stringValue)
        {
            Separator = s_separator!;
            Comparison = s_comparison!.Value;
            var outcome = tryParse(stringValue);
            if (!outcome)
            {
                Items = Array.Empty<string>();
                return base.OnParse(AsError(outcome.Message));
            }

            Items = validate(outcome.Value!);
            IsEmpty = Items.Length == 0;
            return new StringValueParseResult(stringValue!, stringValue?.GetHashCode() ?? 0);
        }

        protected static bool TryParseAs(Type type, string? stringValue, out MultiStringValue? multiStringValue)
        {
            var noParseStringValue = $"{NoParse}{stringValue}";
            multiStringValue = Activator.CreateInstance(type, noParseStringValue) as MultiStringValue;
            if (multiStringValue is null)
                return false;

            var outcome = multiStringValue.tryParse(stringValue);
            return outcome;
        }

        Outcome<string[]> tryParse(string? value)
        {
            if (value.IsUnassigned())
                return Outcome<string[]>.Success(Array.Empty<string>());

            var split = value!.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
                return Outcome<string[]>.Fail(new FormatException($"Invalid {GetType()} format: \"{value}\""));

            var items = new List<string>();
            foreach (var s in split)
            {
                var trimmed = s.Trim();
                var validateOutcome = OnValidateItem(trimmed, s_comparison!.Value);
                if (!validateOutcome)
                    return Outcome<string[]>.Fail(validateOutcome.Exception!);
                
                items.Add(trimmed);
            }
            return Outcome<string[]>.Success(items.ToArray());
        }

        /// <summary>
        ///   Converts a string to its <see cref="MultiStringValue"/>-compatible equivalent.
        ///   A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="stringValue"></param>
        /// <param name="multiStringValue"></param>
        /// <param name="separator">
        ///   (optional)<br/>
        ///   A custom separator.
        /// </param>
        /// <param name="comparison"></param>
        /// <typeparam name="T">
        ///   A <see cref="Type"/>, deriving from <see cref="MultiStringValue"/>,
        ///   for <paramref name="stringValue"/> to be converted to. 
        /// </typeparam>
        /// <returns>
        ///   <c>true</c> if <paramref name="stringValue"/> was converted successfully; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParse<T>(
            string? stringValue,
#if NET5_0_OR_GREATER            
            [NotNullWhen(true)] out T? multiStringValue,
 #else
            out T? multiStringValue,
#endif
            string? separator = null,
            StringComparison? comparison = null)
        where T : MultiStringValue
        {
            // 1st try just calling the 3-arg ctor ...
            var ctor = typeof(T).GetConstructor(new[] { typeof(string), typeof(string), typeof(StringComparison) });
            if (ctor is { })
            {
                try
                {
                    multiStringValue = (T) ctor.Invoke(new object[] { stringValue!, separator!, comparison! });
                    return true;
                }
                catch 
                {
                    // ignored
                }
            }

            // next, try calling the 1-arg ctor ...
            ctor = typeof(T).GetConstructor(new[] { typeof(string) });
            if (ctor is { })
            {
                try
                {
                    multiStringValue = (T) ctor.Invoke(new object[] { stringValue! });
                    return true;
                }
                catch 
                {
                    // ignored
                }
            }

            multiStringValue = null;
            return false;
        }

        internal T WithComparison<T>(StringComparison comparison) where T : MultiStringValue
        {
            Comparison = comparison;
            return (T)this;
        }
        

        public MultiStringValue() : this(string.Empty)
        {
        }

        // note Separator and Comparison are _always_ initialized thru `WithArgs` and Items are always initialized from OnParse 
#pragma warning disable CS8618
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
        /// <param name="comparison">
        ///   Specifies how to perform comparison for <see cref="MultiStringValue"/>s.
        /// </param>
        //[DebuggerStepThrough] 
        public MultiStringValue(
            string? stringValue, 
            string? separator = null, 
            StringComparison comparison = StringComparison.Ordinal) 
        : base(WithArgs(stringValue, separator ?? s_separator ?? DefaultSeparator, comparison))
        {
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
        /// <param name="comparison">
        ///   Specifies how to perform comparison for <see cref="MultiStringValue"/>s.
        /// </param>
        public MultiStringValue(
            IEnumerable<string> items, 
            string? separator = null, 
            StringComparison comparison = StringComparison.Ordinal) 
        : base(WithArgs(items.ConcatEnumerable(), separator, comparison))
        {
        }
#pragma warning restore CS8618
    }
}