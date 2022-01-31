using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using TetraPak.XP.Serialization;

namespace TetraPak.XP.DynamicEntities
{
    [JsonConverter(typeof(JsonStringValueSerializer<DynamicPath>))]
    [DebuggerDisplay("{" + nameof(StringValue) + "}")]
    public class DynamicPath : MultiStringValue
    {
        public const string DefaultPathSeparator = "/";

        string? _stringValue;

        internal List<string> GetStack() => Items!.ToList();

        /// <inheritdoc />
        public override string? StringValue
        {
            get
            {
                if (_stringValue != null)
                    return _stringValue;

                if (this.IsEmpty())
                    return string.Empty;
                
                var sb = new StringBuilder();
                sb.Append(Items![0]);
                for (var i = 1; i < Count; i++)
                {
                    sb.Append(Separator);
                    sb.Append(Items[i]);
                }

                return _stringValue = sb.ToString();
            }
        }

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
        public DynamicPath Push(params string[] items) => Append(items);

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
        public DynamicPath Append(params string[] items)
        {
            AddRange(items);
            invalidateStringValue();
            return this;
        }
        
        /// <summary>
        ///   (fluent API)<br/>
        ///   Creates and returns a modified path by inserting one or more item(s) at the start of the path.
        /// </summary>
        /// <param name="items">
        ///   The items to be inserted.
        /// </param>
        /// <returns>
        ///   The resulting value.
        /// </returns>
        public DynamicPath Insert(params string[] items)
        {
            InsertRange(0, items);
            invalidateStringValue();
            return this;
        }

        /// <summary>
        ///   Pops item(s) from the end of the path and returns the result.<br/>
        ///   (fluent API)
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
            if (Items is null)
                throw new ArgumentOutOfRangeException();
            
            count = Math.Max(0, count);
            if (count == 0)
                return new DynamicPath(Items.ToArray(), Separator);
                
            switch (sequentialPosition)
            {
                case SequentialPosition.Start:
                    while (Count != 0 && count-- != 0)
                    {
                        RemoveAt(0);
                    }
                    break;

                case SequentialPosition.End:
                    while (Count != 0 && count-- != 0)
                    {
                        RemoveAt(Count-1);
                    }
                    break;
                    
                default:
                    throw new NotSupportedException($"Position '{sequentialPosition}' is not supported");
            }

            invalidateStringValue();
            return this;
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
        public static implicit operator string?(DynamicPath value) => value.StringValue;

        /// <inheritdoc />
        public override string ToString() => StringValue ?? string.Empty;
        
        void invalidateStringValue() => _stringValue = null;

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
        public override int GetHashCode()
        {
            return StringValue is {} ? StringValue.GetHashCode() : 0;
        }

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
        ///   (fluent API)<br/>
        ///   Sets the separator, invalidates the <see cref="StringValue"/> and returns <c>this</c>.
        /// </summary>
        public DynamicPath WithSeparator(string separator)
        {
            Separator = separator ?? throw new ArgumentNullException(nameof(separator));
            return this;
        }

        protected virtual void OnConstructStack(string stringValue, string separator)
        {
            SetInternal(stringValue.Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries));
        }

        void constructStack(string stringValue, string separator)
        {
            OnConstructStack(stringValue, separator);
        }

        // protected void SetStack(string[] items) => AddRange(items);
        
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
        /// <exception cref="FormatException">
        ///   The <paramref name="stringValue"/> string representation was incorrectly formed.
        /// </exception>
        [DebuggerStepThrough]
        public DynamicPath(string? stringValue, string? separator = null)
        {
            Separator = separator ?? DefaultPathSeparator;
            if (stringValue is null)
                return;
                
            constructStack(stringValue, Separator);
        }
        
        /// <summary>
        ///   Initializes the value from one or more items.
        /// </summary>
        public DynamicPath(params string[] items)
        : this(items, null!)
        {
        }
        
        public DynamicPath(IEnumerable<string> items, string? separator)
        {
            Separator = separator ?? DefaultPathSeparator;
            AddRange(items);
        }
    }

    public enum FileSystemSeparatorResolutionPolicy
    {
        /// <summary>
        ///   The UNIX file system separator: '/' is preferred.
        /// </summary>
        Unix,
        
        /// <summary>
        ///   The Windows file system separator: '\' is preferred.
        /// </summary>
        Windows,
        
        /// <summary>
        ///   The file system separator that is mostly used is also preferred.
        /// </summary>
        Majority
    }
}