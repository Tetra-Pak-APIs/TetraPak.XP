using System;
using System.Collections.Generic;
using System.Linq;

namespace TetraPak.XP
{
    /// <summary>
    ///   Convenient helper methods for working with <see cref="MultiStringValue"/>s.
    /// </summary>
    public static class MultiStringValueHelper
    {
        // /// <summary>
        // ///   Gets a value indicating whether the <see cref="MultiStringValue"/> is empty.
        // /// </summary>
        // public static bool IsEmpty(this MultiStringValue? self) => self is null || self.Count == 0; obsolete
        
        /// <summary>
        ///   Constructs a new <see cref="MultiStringValue"/> by combining the <see cref="MultiStringValue.Items"/>
        ///   of an existing <see cref="MultiStringValue"/> with another <see cref="MultiStringValue"/>. 
        /// </summary>
        /// <param name="self">
        ///   <c>this</c> <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="other">
        ///   Another <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="trimDuplicates">
        ///   Specifies whether to automatically exclude <see cref="MultiStringValue.Items"/> from <paramref name="other"/>
        ///   that matches items in <c>this</c> <see cref="MultiStringValue"/>. 
        /// </param>
        /// <returns>
        ///   A new <see cref="MultiStringValue"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   The <see cref="MultiStringValue"/> was <c>null</c>.
        /// </exception>
        /// <seealso cref="Join(MultiStringValue,string[],bool)"/>
        public static MultiStringValue Join(this MultiStringValue self, MultiStringValue other, bool trimDuplicates)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
                
            var list = new List<string>(self.Items ?? Array.Empty<string>());
            if (other.Items?.Length == 0)
                return new MultiStringValue(list.ToArray());
            
            list.AddRange(other.Items!);
            var items = list.ToArray();
            return trimDuplicates
                ? MultiStringValue.WithoutDuplicates(items)
                : new MultiStringValue(items);
        }
        
        /// <summary>
        ///   Constructs a new <see cref="MultiStringValue"/> by combining the <see cref="MultiStringValue.Items"/>
        ///   of an existing <see cref="MultiStringValue"/> with a specified array of <see cref="string"/>s. 
        /// </summary>
        /// <param name="self">
        ///   <c>this</c> <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="items">
        ///   An array of <see cref="string"/>s to be combined with the existing <see cref="MultiStringValue.Items"/>.
        /// </param>
        /// <param name="trimDuplicates">
        ///   Specifies whether to automatically exclude <see cref="MultiStringValue.Items"/> from <paramref name="items"/>
        ///   that matches items in <c>this</c> <see cref="MultiStringValue"/>. 
        /// </param>
        /// <returns>
        ///   A new <see cref="MultiStringValue"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   The <see cref="MultiStringValue"/> was <c>null</c>.
        /// </exception>
        /// <seealso cref="Join(MultiStringValue,MultiStringValue,bool)"/>
        public static MultiStringValue Join(this MultiStringValue self, string[]? items, bool trimDuplicates)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            var list = new List<string>(self.Items);
            if (items?.Length == 0)
                return new MultiStringValue(list.ToArray());
            
            list.AddRange(items!);
            return trimDuplicates
                ? MultiStringValue.WithoutDuplicates(list.ToArray())
                : new MultiStringValue(list.ToArray());
        }
        
        /// <summary>
        ///   Creates a new <see cref="MultiStringValue"/> from this one, but without one or more leading element(s).
        /// </summary>
        /// <param name="self">
        ///   The extended <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="count">
        ///   (optional; default=1)<br/>
        ///   Specifies how many items to pop from the <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="safe">
        ///   (optional; default=<c>false</c>)<br/>
        ///   When set an invalid <paramref name="count"/> (too high) an <see cref="MultiStringValue.Empty"/> value is returned;
        ///   otherwise a <see cref="ArgumentOutOfRangeException"/> exception is thrown.
        /// </param>
        /// <returns>
        ///   A <see cref="MultiStringValue"/> with <paramref name="count"/> items removed from the start.
        /// </returns>
        public static MultiStringValue TrimFirst(this MultiStringValue self, int count = 1, bool safe = false)
        {
            count = Math.Max(0, count);
            if (self.IsEmpty || count == 0 || count == self.Count)
                return new MultiStringValue(self.Items);
           
            if (count > self.Items.Length)
                return safe 
                    ? new MultiStringValue()
                    : throw new ArgumentOutOfRangeException(nameof(count), $"Cannot trim {count} items from start of {self}");
            
            var items = new string[self.Items!.Length - count];
            var j = count;
            for (var i = 0; i < items.Length; i++, j++)
            {
                items[i] = self.Items[j];
            }

            return new MultiStringValue(items);
        }

        /// <summary>
        ///   Creates a new <see cref="MultiStringValue"/> from this one, but without one or more trailing element(s).
        /// </summary>
        /// <param name="self">
        ///   The extended <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="count">
        ///   (optional; default=1)<br/>
        ///   Specifies how many items to pop from the <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="safe">
        ///   (optional; default=<c>false</c>)<br/>
        ///   When set an invalid <paramref name="count"/> (too high) an <see cref="MultiStringValue.Empty"/> value is returned;
        ///   otherwise a <see cref="ArgumentOutOfRangeException"/> exception is thrown.
        /// </param>
        /// <returns>
        ///   A <see cref="MultiStringValue"/> with <paramref name="count"/> items removed from the end.
        /// </returns>
        public static MultiStringValue TrimLast(this MultiStringValue self, uint count = 1, bool safe = false)
        {
            count = Math.Max(0, count);
            if (self.IsEmpty || count == 0 || count == self.Count)
                return new MultiStringValue(self.Items);
            
            if (count > self.Items.Length)
                return safe 
                    ? new MultiStringValue()
                    : throw new ArgumentOutOfRangeException(nameof(count), $"Cannot trim {count} items from end of {self}");
            
            var items = new string[self.Items.Length - count];
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = self.Items[i];
            }

            return new MultiStringValue(items);
        }
        
        /// <summary>
        ///   Copies the leading <see cref="MultiStringValue.Items"/> of this value to create a new <see cref="MultiStringValue"/>.
        /// </summary>
        /// <param name="self">
        ///   The extended <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="count">
        ///   (optional; default=1)<br/>
        ///   The number of elements to be copied.
        /// </param>
        /// <param name="safe">
        ///   (optional; default=<c>false</c>)<bt/>
        ///   When set an invalid <paramref name="count"/> value will return an <see cref="MultiStringValue.Empty"/> value;
        ///   otherwise an <see cref="ArgumentOutOfRangeException"/> will be thrown.
        /// </param>
        /// <returns>
        ///   A <see cref="MultiStringValue"/> from the first leading items of this one.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   The <paramref name="count"/> value implied more <see cref="MultiStringValue.Items"/> than was supported by this value,
        ///   and <paramref name="safe"/> was not set.  
        /// </exception>
        public static MultiStringValue CopyFirst(this MultiStringValue self, int count = 1, bool safe = false)
        {
            count = Math.Max(0, count);
            if (self.IsEmpty || count == self.Count)
                return new MultiStringValue(self.Items);
            
            if (count < 1)
                return safe
                    ? new MultiStringValue()
                    : throw new ArgumentOutOfRangeException(nameof(count), $"Cannot extract last {count} items from end of {self}");
            
            if (count > self.Items.Length)
                return safe 
                    ? new MultiStringValue(self.Items)
                    : throw new ArgumentOutOfRangeException(nameof(count), $"Cannot pop {count} items from end of {self}");

            if (count == 1)
                return new MultiStringValue(self.Items[0]);

            var items = new string[count];
            for (var i = 0; i < count; i++)
            {
                items[i] = self.Items[i];
            }

            return new MultiStringValue(items);
        }

        /// <summary>
        ///   Copies the trailing <see cref="MultiStringValue.Items"/> of this value to create a new <see cref="MultiStringValue"/>.
        /// </summary>
        /// <param name="self">
        ///   The extended <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="count">
        ///   (optional; default=1)<br/>
        ///   The number of elements to be copied.
        /// </param>
        /// <param name="safe">
        ///   (optional; default=<c>false</c>)<bt/>
        ///   When set an invalid <paramref name="count"/> value will return an <see cref="MultiStringValue.Empty"/> value;
        ///   otherwise an <see cref="ArgumentOutOfRangeException"/> will be thrown.
        /// </param>
        /// <returns>
        ///   A <see cref="MultiStringValue"/> from the trailing items of this one.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   The <paramref name="count"/> value implied more <see cref="MultiStringValue.Items"/> than was supported by this value,
        ///   and <paramref name="safe"/> was not set.  
        /// </exception>
        public static MultiStringValue CopyLast(this MultiStringValue self, uint count = 1, bool safe = false)
        {
            count = Math.Max(0, count);
            if (count == self.Items.Length)
                return new MultiStringValue(self.Items);
            
            if (count < 1)
                return safe
                    ? new MultiStringValue() 
                    : throw new ArgumentOutOfRangeException(nameof(count), $"Cannot copy {count} items from end of {self}");
            
            if (count > self.Items.Length)
                return safe 
                    ? new MultiStringValue(self.Items)
                    : throw new ArgumentOutOfRangeException(nameof(count), $"Cannot copy {count} items from end of {self}");

            if (count == 1)
                return new MultiStringValue(self.Items[self.Items.Length-1]);

            var items = new string[count];
            var j = 0;
            for (var i = self.Count-count; i < self.Count; i++, j++)
            {
                items[j] = self.Items[i];
            }

            return new MultiStringValue(items);
        }

        /// <summary>
        ///   Determines whether the leading <see cref="MultiStringValue.Items"/>
        ///   matches another <see cref="MultiStringValue"/>. 
        /// </summary>
        /// <param name="self">
        ///   The extended <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="pattern">
        ///   A <see cref="MultiStringValue"/> to compare with.
        /// </param>
        /// <param name="stringComparison">
        ///   (optional; default=<see cref="StringComparison.Ordinal"/>)<br/>
        ///   One of the enumeration values that specifies how the strings will be compared.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the leading <see cref="MultiStringValue.Items"/> of this value matches all
        ///   items of the <paramref name="pattern"/>.
        /// </returns>
        /// <seealso cref="EndsWith"/>
        public static bool StartsWith(
            this MultiStringValue self, 
            MultiStringValue pattern, 
            StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (self.IsEmpty)
                return pattern.IsEmpty;

            if (pattern.IsEmpty)
                return false;
                
            if (pattern.Count > self.Count)
                return false;

            return !pattern.Where((t, i) => !self.Items[i].Equals(pattern.Items[i], stringComparison)).Any();
        }

        /// <summary>
        ///   Determines whether the trailing <see cref="MultiStringValue.Items"/> matches
        ///   another <see cref="MultiStringValue"/>. 
        /// </summary>
        /// <param name="self">
        ///   The extended <see cref="MultiStringValue"/>.
        /// </param>
        /// <param name="pattern">
        ///   A <see cref="MultiStringValue"/> to compare with.
        /// </param>
        /// <param name="stringComparison">
        ///   (optional; default=<see cref="StringComparison.Ordinal"/>)<br/>
        ///   One of the enumeration values that specifies how the strings will be compared.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the trailing <see cref="MultiStringValue.Items"/> of this value matches all
        ///   items of the <paramref name="pattern"/>.
        /// </returns>
        /// <seealso cref="StartsWith"/>
        public static bool EndsWith(
            this MultiStringValue self, 
            MultiStringValue pattern, 
            StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (self.IsEmpty)
                return pattern.IsEmpty;

            if (pattern.IsEmpty)
                return false;
            
            if (pattern.Count > self.Count)
                return false;

            var j = self.Count - pattern.Count;
            for (var i = 0; i < pattern.Count; i++, j++)
            {
                if (!self.Items[j].Equals(pattern.Items[i], stringComparison))
                    return false;
            }

            return true;
        }
    }
}