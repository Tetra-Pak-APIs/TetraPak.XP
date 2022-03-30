using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using TetraPak.XP.Serialization;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.DynamicEntities
{
    [JsonConverter(typeof(JsonStringValueSerializer<DynamicPath>))]
    [DebuggerDisplay("{" + nameof(StringValue) + "}")]
    public class DynamicPath : MultiStringValue
    {
        public const string DefaultPathSeparator = "/";

        /// <summary>
        ///   Gets the first path element (or an empty <see cref="string"/> if <see cref="MultiStringValue.IsEmpty"/>).
        /// </summary>
        public string Root => Count > 0 ? Items[0] : string.Empty;

        /// <summary>
        ///   Gets the last path element (or an empty <see cref="string"/> if <see cref="MultiStringValue.IsEmpty"/>).
        /// </summary>
        public string Leaf => Count > 0 ? Items[Count - 1] : string.Empty;

        /// <summary>
        ///   (fluent api)<br/>
        ///   Creates and returns a modified path by adding one or more item(s) to the end of the path.
        ///   (This method simply calls <see cref="Append"/>).
        /// </summary>
        /// <param name="items">
        ///   The items to be added.
        /// </param>
        /// <returns>
        ///   The resulting value.
        /// </returns>
        public DynamicPath Push(params string[] items) => OnCreate(AddRange(items));

        /// <summary>
        ///   (fluent api)<br/>
        ///   Creates and returns a modified path by adding one or more item(s) to the end of the path.
        /// </summary>
        /// <param name="items">
        ///   The items to be added.
        /// </param>
        /// <returns>
        ///   The resulting value.
        /// </returns>
        public DynamicPath Append(params string[] items) => OnCreate(AddRange(items));

        /// <summary>
        ///   (fluent api)<br/>
        ///   Creates and returns a modified path by inserting one or more item(s) at the start of the path.
        /// </summary>
        /// <param name="items">
        ///   The items to be inserted.
        /// </param>
        /// <returns>
        ///   The resulting value.
        /// </returns>
        public DynamicPath Insert(params string[] items) => OnCreate(InsertRange(0, items));

        protected virtual DynamicPath OnCreate(string[] items) => new(items, Separator, Comparison);

        /// <summary>
        ///   Pops item(s) from the end of the path and returns the result.<br/>
        ///   (fluent api)
        /// </summary>
        /// <param name="count">
        ///   (optional; default=1)<br/>
        ///   Specifies the number of items to remove from end of path.
        /// </param>
        /// <param name="sequentialPosition">
        ///   (optional; default=<see cref="SequentialPosition.End"/>)<br/>
        ///   Specifies whether to pop elements from the end or start of the path.
        /// </param>
        /// <returns>
        ///   The resulting value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   The path was empty.
        /// </exception>
        public DynamicPath Pop(int count = 1, SequentialPosition sequentialPosition = SequentialPosition.End)
        {
            if (IsEmpty)
                throw new ArgumentOutOfRangeException();
            
            count = Math.Max(0, count);
            if (count == 0)
                return new DynamicPath(Items, Separator);

            return sequentialPosition switch
            {
                SequentialPosition.Start => new DynamicPath(RemoveAt(0, count), Separator, Comparison),
                SequentialPosition.End => new DynamicPath(RemoveAt(Count - count, count), Separator, Comparison),
                _ => throw new NotSupportedException($"Position '{sequentialPosition}' is not supported")
            };
        }
        
        /// <summary>
        ///   Implicitly converts a string literal into a <see cref="DynamicPath"/>.
        /// </summary>
        /// <param name="stringValue">
        ///   A string representation of the <see cref="DynamicPath"/> value.
        /// </param>
        /// <returns>
        ///   A <see cref="DynamicPath"/> value.
        /// </returns>
        /// <exception cref="FormatException">
        ///   The <paramref name="stringValue"/> string representation was incorrectly formed.
        /// </exception>
        public static implicit operator DynamicPath(string? stringValue) 
            => 
            stringValue is {} ? new DynamicPath(stringValue) : null!;

        /// <summary>
        ///   Implicitly converts a <see cref="DynamicPath"/> value into its <see cref="string"/> representation.
        /// </summary>
        /// <param name="value">
        ///   A <see cref="DynamicPath"/> value to be implicitly converted into its <see cref="string"/> representation.
        /// </param>
        /// <returns>
        ///   The <see cref="string"/> representation of <paramref name="value"/>.
        /// </returns>
        public static implicit operator string(DynamicPath value) => value.StringValue;

        /// <inheritdoc />
        public override string ToString() => Items.ConcatCollection(Separator);
        
        #region .  Equality  .

        /// <summary>
        ///   Determines whether the specified value is equal to the current value.
        /// </summary>
        /// <param name="other">
        ///   A <see cref="DynamicPath"/> value to compare to this value.
        /// </param>
        /// <param name="stringComparison">
        ///   Specifies the string comparison strategy.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="other"/> is equal to the current value; otherwise <c>false</c>.
        /// </returns>
        public bool Equals(DynamicPath? other, StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            return other is {} && string.Equals(StringValue, other.StringValue, stringComparison);
        }

        /// <summary>
        ///   Determines whether the specified string collection matches the current value.
        /// </summary>
        /// <param name="items">
        ///   The string items to compare this value to.
        /// </param>
        /// <param name="stringComparison">
        ///   Specifies the string comparison strategy.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the <paramref name="items"/> matches the internal items of the current value; otherwise <c>false</c>.
        /// </returns>
        public bool Equals(IEnumerable<string>? items, StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            return items is {} && Equals(new DynamicPath(items.ToArray()), stringComparison);
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
            return obj is DynamicPath other && Equals(other);
        }

        /// <summary>
        ///   Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///   A hash code for the current value.
        /// </returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        ///   Comparison operator overload.
        /// </summary>
        public static bool operator ==(DynamicPath? left, DynamicPath? right)
        {
            return left?.Equals(right) ?? right is null;
        }

        /// <summary>
        ///   Comparison operator overload.
        /// </summary>
        public static bool operator !=(DynamicPath? left, DynamicPath? right)
        {
            return !left?.Equals(right) ?? right is { };
        }

        #endregion

        /// <summary>
        ///   (fluent api)<br/>
        ///   Sets the separator and returns <c>this</c>.
        /// </summary>
        public DynamicPath WithSeparator(string separator)
        {
            if (separator.IsUnassigned())
                throw new ArgumentNullException(nameof(separator));

            Separator = separator;
            return this;
        }

        /// <summary>
        ///   Initializes the value.
        /// </summary>
        /// <param name="stringValue">
        ///   The new value's string representation (will automatically be parsed).
        /// </param>
        /// <param name="separator">
        ///   (optional)<br/>
        ///   Specifies a (custom) separator. 
        /// </param>
        /// <param name="comparison">
        ///   Specifies how to perform string comparison.
        /// </param>
        /// <exception cref="FormatException">
        ///   The <paramref name="stringValue"/> string representation was incorrectly formed.
        /// </exception>
        [DebuggerStepThrough]
        public DynamicPath(string? stringValue, string? separator = null, StringComparison comparison = StringComparison.Ordinal)
        : base(WithArgs(stringValue, separator ?? DefaultPathSeparator, comparison))
        {
        }
        
        /// <summary>
        ///   Initializes the value from one or more items,
        /// with default <see cref="MultiStringValue.Separator"/> and <see cref="MultiStringValue.Comparison"/>.
        /// </summary>
        /// <seealso cref="DynamicPath(IEnumerable{string}, string?, StringComparison)"/>
        /// <param name="items">
        ///   The elements to make up the path.
        /// </param>
        public DynamicPath(params string[] items)
        : this(items, null!)
        {
        }
        
        /// <summary>
        ///   Initializes the value from one or more items.
        /// </summary>
        /// <param name="items">
        ///   The elements to make up the path.
        /// </param>
        /// <param name="separator">
        ///   (optional)<br/>
        ///   The separator token to be used in sting representations for the path. 
        /// </param>
        /// <param name="comparison">
        ///   (optional; default=<see cref="StringComparison.Ordinal"/>)<br/>
        ///   Specifies how to perform comparison for this path. 
        /// </param>
        public DynamicPath(
            IEnumerable<string> items, 
            string? separator = null, 
            StringComparison comparison = StringComparison.Ordinal)
        : base(WithArgs(items.ConcatCollection(separator ?? DefaultPathSeparator), separator, comparison))
        {
        }
    }
}