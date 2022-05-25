using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TetraPak.XP
{
    /// <summary>
    ///   Convenient extension methods for <see cref="string"/> operations.
    /// </summary>
    public static class StringHelper
    {
        public static bool StartsWith(this string self, char c, bool ignoreCase = false) 
        => self.IsAssigned() && self[0].Equals(c, ignoreCase);

        public static bool StartsWithAny(this string self, IEnumerable<char> chars, bool ignoreCase = false) 
        => chars.Any(c => self.StartsWith(c, ignoreCase));

        public static bool EndsWith(this string self, char c, bool ignoreCase = false) 
        => self.IsAssigned() && self[self.Length-1].Equals(c, ignoreCase);

        public static bool EndsWithAny(this string self, IEnumerable<char> chars, bool ignoreCase = false) 
        => chars.Any(c => self.EndsWith(c, ignoreCase));

#if !NET5_0_OR_GREATER 
        public static bool Contains(this string self, string match, StringComparison comparison)
        {
            return self.IndexOf(match, comparison) != -1;
            
            // if (match.Length == 0)
            //     return self.Contains(match);
            //
            // if (match.Length > self.Length)
            //     return false;
            //
            // // 0 1 2 3 4 5 6 7 8 9 
            // // a B c D e F g H i J
            // //             g h i j
            // var caSelf = self.ToCharArray();
            // var caMatch = match.ToCharArray();
            // var ignoreCase = comparison 
            //     is StringComparison.OrdinalIgnoreCase
            //     or StringComparison.CurrentCultureIgnoreCase or StringComparison.InvariantCultureIgnoreCase;
            // for (var i = 0; i <= caSelf.Length-match.Length; i++)
            // {
            //     if (!caSelf[i].Equals(caMatch[0], ignoreCase))
            //         continue;
            //
            //     var j = 1;
            //     for (; j < caMatch.Length; j++)
            //     {
            //         if (!caSelf[i+j].Equals(caMatch[j]))
            //             break;
            //     }
            //
            //     if (j == caMatch.Length)
            //         return true;
            // }
            //
            // return false;
        }
#endif

        // todo Copy to TetraPak.Common (.NET 5+ / Core)
        public static string EnsureAssigned(
            this string? self,
            string identifier,
            string? useDefault, 
            Func<string>? fallback = null,
            bool allowWhitespace = false) 
            => self.EnsureAssigned(
                useDefault,
                new Exception($"{identifier} was not assigned"),
                fallback,
                allowWhitespace);

        // todo Copy to TetraPak.Common (.NET 5+ / Core)
        public static string EnsureAssigned(
            this string? self,
            string? useDefault, 
            Exception throwWenUnassigned,
            Func<string>? fallback = null,
            bool allowWhitespace = false)
        {
            var result = self ?? useDefault ?? fallback?.Invoke();
            if (result is null || !allowWhitespace && result.IsWhitespace())
                throw throwWenUnassigned;

            return result;
        }
        
        /// <summary>
        ///   Examines the string and returns a value to indicate it is a non-<c>null</c> value  of one or
        ///   more whitespace characters.
        /// </summary>
        public static bool IsWhitespace(this string? self)
        {
            if (self is null || self.Length == 0)
                return false;

            var ca = self.ToCharArray();
            for (var i = 0; i < ca.Length; i++)
            {
                if (!char.IsWhiteSpace(ca[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        ///   Examines the string and returns a value to indicate it a non-<c>null</c> value that is
        ///   either of zero length or contains only whitespace characters.
        /// </summary>
        public static bool IsWhitespaceOrEmpty(this string? self)
        {
            if (self is null)
                return false;

            return self.Length == 0 || self.IsWhitespace();
        }
        
        /// <summary>
        ///   Ensures the first letter in the string is lowercase.
        /// </summary>
        /// <param name="self">
        ///   The string to be transformed.
        /// </param>
        /// <param name="findFirstLetter">
        ///   (optional; default = <c>false</c>)<br/>
        ///   When set; the first occurence if a letter is automatically found (and transformed). 
        /// </param>
        /// <returns>
        ///   The transformed string.
        /// </returns>
        public static string ToLowerInitial(this string self, bool findFirstLetter = false)
        {
            if (string.IsNullOrEmpty(self))
                return self;

            // try quick-win scenario ...
            if (char.IsLetter(self[0]))
                return self.Length > 1
                    ? $"{char.ToLower(self[0]).ToString()}{self.Substring(1)}"
                    : char.ToLower(self[0]).ToString();

            // first char is not a letter ...
            if (!findFirstLetter)
                return self;

            var ca = self.ToCharArray();
            var sb = new StringBuilder();
            sb.Append(ca[0]);
            var i = 1;
            for (; i < ca.Length; i++)
            {
                if (char.IsLetter(ca[i]))
                {
                    sb.Append(char.ToLower(ca[i++]));
                    break;
                }

                sb.Append(ca[i]);
            }

            if (i < ca.Length)
                sb.Append(self.Substring(i));

            return sb.ToString();
        }

        /// <summary>
        ///   Ensures the first letter in the string is uppercase.
        /// </summary>
        /// <param name="self">
        ///   The string to be transformed.
        /// </param>
        /// <param name="findFirstLetter">
        ///   (optional; default = <c>false</c>)<br/>
        ///   When set; the first occurence if a letter is automatically found (and transformed). 
        /// </param>
        /// <returns>
        ///   The transformed string.
        /// </returns>
        public static string ToUpperInitial(this string self, bool findFirstLetter = false)
        {
            if (string.IsNullOrEmpty(self))
                return self;

            // try quick-win scenario ...
            if (char.IsLetter(self[0]))
                return self.Length > 1
                    ? $"{char.ToUpper(self[0]).ToString()}{self.Substring(1)}"
                    : char.ToUpper(self[0]).ToString();

            // first char is not a letter ...
            if (!findFirstLetter)
                return self;

            var ca = self.ToCharArray();
            var sb = new StringBuilder();
            sb.Append(ca[0]);
            var i = 1;
            for (; i < ca.Length; i++)
            {
                if (char.IsLetter(ca[i]))
                {
                    sb.Append(char.ToUpper(ca[i++]));
                    break;
                }

                sb.Append(ca[i]);
            }

            if (i < ca.Length)
                sb.Append(self.Substring(i));

            return sb.ToString();
        }

        /// <summary>
        ///   Splits a camel-cased string at every capital letter and returns the individual words. 
        /// </summary>
        public static string SplitCamelCase(this string self)
        {
            var ca = self.Trim().ToCharArray();
            var sb = new StringBuilder();
            for (var i = 0; i < ca.Length; i++)
            {
                var c = ca[i];
                if (i == 0)
                {
                    sb.Append(c);
                    continue;
                }

                if (char.IsUpper(c))
                {
                    sb.Append(' ');
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        public static bool IsPatternMatch(
            this string self, 
            string test,
            StringComparison comparison = StringComparison.Ordinal, 
            string replaceAllWildcard = "*",
            string replaceOneWildcard = "?")
        {
            string? regexPattern = null;

            if (!string.IsNullOrWhiteSpace(replaceOneWildcard))
                regexPattern = self.Replace(replaceOneWildcard, ".?");

            if (!string.IsNullOrWhiteSpace(replaceAllWildcard))
                regexPattern = self.Replace(replaceAllWildcard, ".*");

            if (regexPattern is null)
                return self.Equals(test, comparison);

            var options = RegexOptions.None;
            switch (comparison)
            {
                case StringComparison.InvariantCulture or StringComparison.InvariantCultureIgnoreCase:
                {
                    options = RegexOptions.CultureInvariant;
                    if (comparison == StringComparison.InvariantCultureIgnoreCase)
                        options |= RegexOptions.IgnoreCase;
                    break;
                }
                case StringComparison.CurrentCultureIgnoreCase 
                    or StringComparison.InvariantCultureIgnoreCase 
                    or StringComparison.OrdinalIgnoreCase:
                    options = RegexOptions.IgnoreCase;
                    break;
            }

            var regex = new Regex(regexPattern, options);
            return regex.IsMatch(test);
        }

        /// <summary>
        ///   Returns the string prefixed as specified. 
        /// </summary>
        /// <param name="self">
        ///   The string to be prefixed.
        /// </param>
        /// <param name="prefix">
        ///   The required postfix.
        /// </param>
        /// <param name="comparison">
        ///   (optional; default=<see cref="StringComparison.Ordinal"/>)<br/>
        ///   Specifies how to match the postfix.
        /// </param>
        /// <returns>
        ///   The string, prefixed as specified.
        /// </returns>
        /// <seealso cref="EnsurePrefix(string,char,bool)"/>
        /// <seealso cref="EnsurePostfix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePostfix(string,char,bool)"/>
        /// <seealso cref="TrimPrefix(string,char,bool)"/>
        /// <seealso cref="TrimPostfix(string,char,bool)"/>
        public static string EnsurePrefix(
            this string self, 
            string prefix,
            StringComparison comparison = StringComparison.Ordinal)
        {
            return self.StartsWith(prefix, comparison) 
                ? self
                : $"{prefix}{self}";
        }

        /// <summary>
        ///   Returns the string prefixed as specified. 
        /// </summary>
        /// <param name="self">
        ///   The string to be prefixed.
        /// </param>
        /// <param name="prefix">
        ///   The required postfix.
        /// </param>
        /// <param name="ignoreCase">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies how to match the postfix.
        /// </param>
        /// <returns>
        ///   The string, prefixed as specified.
        /// </returns>
        /// <seealso cref="EnsurePrefix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePostfix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePostfix(string,char,bool)"/>
        /// <seealso cref="TrimPrefix(string,char,bool)"/>
        /// <seealso cref="TrimPostfix(string,char,bool)"/>
        public static string EnsurePrefix(
            this string self, 
            char prefix,
            bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(self))
                return new string(prefix, 1);
            
            return self[0].Equals(prefix, ignoreCase) 
                ? self
                : $"{prefix.ToString()}{self}";
        }

        /// <summary>
        ///   Returns the string prefixed as specified. 
        /// </summary>
        /// <param name="self">
        ///   The string to be prefixed.
        /// </param>
        /// <param name="postfix">
        ///   The required postfix.
        /// </param>
        /// <param name="comparison">
        ///   (optional; default=<see cref="StringComparison.Ordinal"/>)<br/>
        ///   Specifies how to match the postfix.
        /// </param>
        /// <returns>
        ///   The string, prefixed as specified.
        /// </returns>
        /// <seealso cref="EnsurePrefix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePrefix(string,char,bool)"/>
        /// <seealso cref="EnsurePostfix(string,char,bool)"/>
        /// <seealso cref="TrimPrefix(string,char,bool)"/>
        /// <seealso cref="TrimPostfix(string,char,bool)"/>
        public static string EnsurePostfix(
            this string self, 
            string postfix,
            StringComparison comparison = StringComparison.Ordinal)
        {
            return self.EndsWith(postfix, comparison)
                ? self 
                : $"{self}{postfix}";
        }
        
        /// <summary>
        ///   Returns the string prefixed as specified. 
        /// </summary>
        /// <param name="self">
        ///   The string to be prefixed.
        /// </param>
        /// <param name="postfix">
        ///   The required postfix.
        /// </param>
        /// <param name="ignoreCase">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies how to match the postfix.
        /// </param>
        /// <returns>
        ///   The string, prefixed as specified.
        /// </returns>
        /// <seealso cref="EnsurePrefix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePrefix(string,char,bool)"/>
        /// <seealso cref="EnsurePostfix(string,string,StringComparison)"/>
        /// <seealso cref="TrimPrefix(string,char,bool)"/>
        /// <seealso cref="TrimPostfix(string,char,bool)"/>
        public static string EnsurePostfix(
            this string self, 
            char postfix,
            bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(self))
                return new string(postfix, 1);
            
            return self[self.Length-1].Equals(postfix, ignoreCase) 
                ? self
                : $"{self}{postfix.ToString()}";
        }

        /// <summary>
        ///   Returns the string without a specified prefix (when present). 
        /// </summary>
        /// <param name="self">
        ///   The string to remove prefix from.
        /// </param>
        /// <param name="prefix">
        ///   The prefix to be removed.
        /// </param>
        /// <param name="comparison">
        ///   (optional; default=<see cref="StringComparison.Ordinal"/>)<br/>
        ///   Specifies how to match the prefix.
        /// </param>
        /// <returns>
        ///   The <see cref="string"/> without the specified prefix.
        /// </returns>
        /// <seealso cref="TrimPrefix(string,char,bool)"/>
        /// <seealso cref="TrimPostfix(string,char,bool)"/>
        /// <seealso cref="EnsurePrefix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePrefix(string,char,bool)"/>
        /// <seealso cref="EnsurePostfix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePostfix(string,char,bool)"/>
        public static string TrimPrefix(
            this string self, 
            string prefix,
            StringComparison comparison = StringComparison.Ordinal)
        {
            return !self.StartsWith(prefix, comparison)
                ? self 
                : self.Remove(0, prefix.Length);
        }
        
        /// <summary>
        ///   Returns the string without a specified prefix <see cref="char"/> (if present). 
        /// </summary>
        /// <param name="self">
        ///   The string to remove prefix from.
        /// </param>
        /// <param name="prefix">
        ///   The prefix to be removed.
        /// </param>
        /// <param name="ignoreCase">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies whether to ignore letter casing when matching the prefix.
        /// </param>
        /// <returns>
        ///   The <see cref="string"/> without the specified prefix.
        /// </returns>
        /// <seealso cref="TrimPostfix(string,char,bool)"/>
        /// <seealso cref="EnsurePrefix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePrefix(string,char,bool)"/>
        /// <seealso cref="EnsurePostfix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePostfix(string,char,bool)"/>
        public static string TrimPrefix(
            this string self, 
            char prefix,
            bool ignoreCase = false)
        {
            return string.IsNullOrEmpty(self) || !self[0].Equals(prefix, ignoreCase) 
                ? self 
                : self.Substring(1);
        }

        /// <summary>
        ///   Returns the string without a specified postfix (when present). 
        /// </summary>
        /// <param name="self">
        ///   The string to remove postfix from.
        /// </param>
        /// <param name="postfix">
        ///   The postfix to be removed.
        /// </param>
        /// <param name="comparison">
        ///   (optional; default=<see cref="StringComparison.Ordinal"/>)<br/>
        ///   Specifies how to match the postfix.
        /// </param>
        /// <returns>
        ///   The <see cref="string"/> without the specified postfix.
        /// </returns>
        /// <seealso cref="TrimPrefix(string,char,bool)"/>
        /// <seealso cref="TrimPostfix(string,char,bool)"/>
        /// <seealso cref="EnsurePrefix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePrefix(string,char,bool)"/>
        /// <seealso cref="EnsurePostfix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePostfix(string,char,bool)"/>
        public static string TrimPostfix(
            this string self, 
            string postfix,
            StringComparison comparison = StringComparison.Ordinal)
        {
            return !self.EndsWith(postfix, comparison)
                ? self 
                : self.Substring(0, self.Length - postfix.Length);
        }
        
        /// <summary>
        ///   Returns the string without a specified postfix <see cref="char"/> (if present). 
        /// </summary>
        /// <param name="self">
        ///   The string to remove postfix from.
        /// </param>
        /// <param name="postfix">
        ///   The postfix to be removed.
        /// </param>
        /// <param name="ignoreCase">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies whether to ignore letter casing when matching the postfix.
        /// </param>
        /// <returns>
        ///   The <see cref="string"/> without the specified postfix.
        /// </returns>
        /// <seealso cref="TrimPrefix(string,char,bool)"/>
        /// <seealso cref="EnsurePrefix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePrefix(string,char,bool)"/>
        /// <seealso cref="EnsurePostfix(string,string,StringComparison)"/>
        /// <seealso cref="EnsurePostfix(string,char,bool)"/>
        public static string TrimPostfix(
            this string self, 
            char postfix,
            bool ignoreCase = false)
        {
            return string.IsNullOrEmpty(self) || !self[self.Length-1].Equals(postfix, ignoreCase) 
                ? self 
                : self.Substring(1);
        }

        /// <summary>
        ///   Constructs and returns a new <see cref="string"/> by replacing the last <see cref="char"/>.
        /// </summary>
        /// <param name="self">
        ///   The original <see cref="string"/>.
        /// </param>
        /// <param name="replace">
        ///   The <see cref="char"/> ro replace the last one of the <see cref="string"/>.
        /// </param>
        /// <returns>
        ///   The new <see cref="string"/>.
        /// </returns>
        public static string ReplaceLastChar(this string self, char replace)
        {
            var lastIndex = self.Length - 1;
            if (lastIndex == 0)
                return (char.IsUpper(self[lastIndex]) ? char.ToUpper(replace) : char.ToLower(replace)).ToString();

            var sb = new StringBuilder(self.Substring(0, self.Length - 2));
            sb.Append(char.IsUpper(self[lastIndex]) ? char.ToUpper(replace) : char.ToLower(replace));
            return sb.ToString();
        }

        // public static string ReplaceEnding(
        //     this string self, 
        //     string ending, 
        //     string replace,
        //     StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
        // {
        //     if (!self.EndsWith(ending, comparison))
        //         return self;
        //
        //     var i = self.Length - ending.Length;
        //     var sb = new StringBuilder(self[..^i]);
        //     for (var j = 0; j < ending.Length; i++, j++)
        //     {
        //         var c = self[i];
        //         var e = ending[j];
        //         var isLower = char.IsLower(c);
        //         sb.Append(isLower ? char.ToLower(e) : char.ToUpper(e));
        //     }
        //
        //     return sb.ToString();
        // }

        /// <summary>
        ///   Examines a <see cref="char"/> and returns a value indicating it is a vowel.
        /// </summary>
        /// <seealso cref="IsConsonant"/>
        public static bool IsVowel(this char c)
        {
            c = char.ToLower(c);
            return c is 'a' or 'e' or 'i' or 'o' or 'u' or 'y';
        }

        /// <summary>
        ///   Examines a <see cref="char"/> and returns a value indicating it is a consonant.
        /// </summary>
        /// <seealso cref="IsVowel"/>
        public static bool IsConsonant(this char c) => !IsVowel(c);

        public static bool Equals(this char self, char match, bool ignoreCase)
        {
            if (ignoreCase)
                return char.IsUpper(self) ? self == char.ToUpper(match) : self == char.ToLower(match);

            return self == match;
        }


        /// <summary>
        ///   Constructs a textual representation of a collection of values.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="separator"></param>
        /// <param name="callback"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string ConcatEnumerable<T>(
            this IEnumerable<T>? values,
            string separator = ", ",
            Func<object?, string>? callback = null,
            int offset = 0)
        {
            var a = values?.ToArray();
            if (a is null || a.Length == 0)
                return string.Empty;
                
            var sb = new StringBuilder();
            var value = a[offset];
            sb.Append(callback?.Invoke(value) ?? SafeToString(value));
            for (var i = offset + 1; i < a.Length; i++)
            {
                sb.Append(separator);
                value = a[i];
                sb.Append(callback?.Invoke(value) ?? SafeToString(value));
            }

            return sb.ToString();
        }

        public static string ConcatDictionary<TKey, TValue>(
            this IDictionary<TKey, TValue>? self,
            string separator = ", ",
            Func<object, string>? callback = null) where TKey : notnull
        {
            var a = self?.ToArray();
            return Concat(a, separator, callback);
        }

        /// <summary>
        ///   Builds a <see cref="string"/> value from a collection of key/value pairs.
        /// </summary>
        /// <param name="self">
        ///   The collection of key value pairs.
        /// </param>
        /// <param name="separator">
        ///   (optional; default=", ")<br/>
        ///   A separator to be used for separating the individual items in the result.
        /// </param>
        /// <param name="callback">
        ///   (optional)<br/>
        ///   A callback method to customize the result (will be called once per item to produce a <see cref="string"/>
        ///   representation for that item.
        /// </param>
        /// <typeparam name="TKey">
        ///   The key value pair key type.
        /// </typeparam>
        /// <typeparam name="TValue">
        ///   The key value pair value type.
        /// </typeparam>
        /// <returns>
        ///   A <see cref="string"/> representation of the collection.
        /// </returns>
        public static string Concat<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>>? self,
            string separator = ", ",
            Func<object, string>? callback = null)
        {
            var a = self?.ToArray();
            if (a is null || a.Length == 0)
                return string.Empty;
            
            var sb = new StringBuilder();
            sb.Append(callback?.Invoke(a[0]) ?? safeToString(a[0]));
            for (var i = 1; i < a.Length; i++)
            {
                sb.Append(separator);
                sb.Append(callback?.Invoke(a[i]) ?? safeToString(a[i]));
            }

            return sb.ToString();

            string safeToString(KeyValuePair<TKey, TValue> pair)
            {
                return $"{pair.Key}={SafeToString(pair.Value, separator:separator)}";
            }
        }

        /// <summary>
        ///   An alternative to the default <see cref="object.ToString"/> to always return a string,
        ///   even for <c>null</c> values.  
        /// </summary>
        /// <param name="value">
        ///   The object to be returned in its textual representation.
        /// </param>
        /// <param name="nullIdentifier">
        ///   (optional; default=<c>"(null)"</c>)<br/>
        ///   A string literal to be returned if the <paramref name="value"/> is <c>null</c>.
        /// </param>
        /// <param name="separator">
        ///   (optional; default=<c>", "</c>)<br/>
        ///   A string literal to be used as separator when the value is a collection of values.
        /// </param>
        /// <returns>
        ///   The <paramref name="value"/> in its textual representation.
        /// </returns>
        public static string SafeToString(
            this object? value, 
            string nullIdentifier = "(null)", 
            string separator = ", ")
        {
            if (ReferenceEquals(null, value))
                return nullIdentifier;

            if (!value.IsCollection(out _, out var items, out _)) 
                return value.ToString() ?? nullIdentifier;
            
            var enumerator = items!.GetEnumerator();
            if (!enumerator.MoveNext())
                return string.Empty;
                
            var sb = new StringBuilder();
            sb.Append(SafeToString(enumerator.Current, separator:separator));
            while (enumerator.MoveNext())
            {
                sb.Append(separator);
                sb.Append(SafeToString(enumerator.Current, separator:separator));
            }

            return sb.ToString();
        }

        public static bool EqualsSubstring(this string self, int startIndex, int length, string match,
            StringComparison comparison)
        {
            if (startIndex < 0)
                startIndex = self.Length + startIndex;

            return self.Substring(startIndex, length).Equals(match, comparison);
        }

        /// <summary>
        ///   Looks for a string in an array of strings and returns its internal position. 
        /// </summary>
        /// <param name="array">
        ///   The array to be examined.
        /// </param>
        /// <param name="pattern">
        ///   The textual pattern to look for.
        /// </param>
        /// <param name="stringComparison">
        ///   
        /// </param>
        /// <returns>
        ///   A positive number, specifying the matching string's index in the array, on a successful match;
        ///   otherwise a negative value (-1). 
        /// </returns>
        /// <seealso cref="IndexOf(string[],Regex)"/>
        public static int IndexOf(
            this string[] array, 
            string pattern,
            StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            for (var i = 0; i < array.Length; i++)
            {
                var s = array[i];
                if (s.Equals(pattern, stringComparison))
                    return i;
            }

            return -1;
        }
        
        /// <summary>
        ///   Looks for a string in an array of strings, using <see cref="Regex"/> matching,
        ///   and returns its internal position. 
        /// </summary>
        /// <param name="array">
        ///   The array to be examined.
        /// </param>
        /// <param name="pattern">
        ///   The <see cref="Regex"/> pattern to match.
        /// </param>
        /// <returns>
        ///   A positive number, specifying the matching string's index in the array, on a successful match;
        ///   otherwise a negative value (-1). 
        /// </returns>
        /// <seealso cref="IndexOf(string[],string,System.StringComparison)"/>
        public static int IndexOf(
            this string[] array, 
            Regex pattern)
        {
            for (var i = 0; i < array.Length; i++)
            {
                var s = array[i];
                if (pattern.IsMatch(s))
                    return i;
            }

            return -1;
        }

        /// <summary>
        ///   Returns the string in its base-64 encoded form.
        /// </summary>
        public static string ToBase64String(this string self) => Convert.ToBase64String(Encoding.UTF8.GetBytes(self));

        /// <summary>
        ///   Produces and returns a <see cref="MemoryStream"/> from the string.
        /// </summary>
        public static Stream ToStream(this string self) => new MemoryStream(Encoding.ASCII.GetBytes(self));

        public static string GetMatchingPrefix(this string self, string compare)
        {
            if (string.IsNullOrEmpty(self) || string.IsNullOrEmpty(compare))
                return string.Empty;

            var length = Math.Min(self.Length, compare.Length);
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                if (self[i] != compare[i])
                    return sb.ToString();

                sb.Append(self[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        ///   Examines the string and returns a value indicating whether it only contains numerics.
        /// </summary>
        /// <param name="self">
        ///   The string to be examined.
        /// </param>
        /// <param name="allowedSymbols">
        ///   (optional)<br/>
        ///   One or more allowed (non-numeric) symbols, such as sign or delimiters.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the string only contains numeric characters and, optionally,
        ///   any <paramref name="allowedSymbols"/>; otherwise <c>false</c>.
        /// </returns>
        public static bool IsNumericsOnly(this string self, params char[] allowedSymbols)
        {
            return self.All(c => char.IsNumber(c) || allowedSymbols.Any(i => i == c));
        }

        /// <summary>
        ///   Examines the string and returns a value to indicate whether it is considered to be assigned.
        /// </summary>
        /// <param name="self">
        ///   The string to be examined.
        /// </param>
        /// <param name="allowWhitespace">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies whether a string with only whitespace is considered to be assigned.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the string is considered to be assigned; otherwise <c>false</c>.
        /// </returns>
        public static bool IsAssigned(this string? self, bool allowWhitespace = false)
        {
            if (self is null)
                return false;
            
            if (self.Length == 0)
                return false;
            
            return allowWhitespace || self.Any(c => !char.IsWhiteSpace(c));
        }

        public static bool IsUnassigned(this string? self, bool allowWhitespace = false) =>
            !self.IsAssigned(allowWhitespace);

        /// <summary>
        ///   Constructs and returns the string trimmed of all leading/trailing whitespace.
        /// </summary>
        public static string TrimWhitespace(this string self)
        {
            var sb = new StringBuilder();
            var ca = self.ToCharArray();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < ca.Length; i++)
            {
                var c = ca[i];
                if (!char.IsWhiteSpace(c))
                    sb.Append(c);

            }

            return sb.ToString();
        }

        /// <summary>
        ///   Examines a collection of strings and returns it as an array without any duplicates.
        /// </summary>
        /// <param name="strings">
        ///   The collection of strings.
        /// </param>
        /// <returns>
        ///   An array of strings, where all duplicates have been removed.
        /// </returns>
        /// <remarks>
        ///   The internal matching simply relies on the string items' hash values.
        /// </remarks>
        public static string[] ToNormalizedArray(this IEnumerable<string> strings)
        {
            var array = strings.ToArray();
            var hash = new HashSet<string>();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < array.Length; i++)
            {
                var s = array[i];
                if (!hash.Contains(s))
                    hash.Add(s);
            } 
            
            return hash.ToArray();
        }

        public static bool TryEatEnclosed(
            this string? text, 
            string prefix, 
            string suffix, 
#if NET5_0_OR_GREATER            
            [NotNullWhen(true)]
#endif            
            out string? enclosed, 
            int startIndex = 0, 
            StringComparison comparison = StringComparison.Ordinal)
        {
            enclosed = null;
            if (text.IsUnassigned())
                return false;

            var idxPrefix = text!.IndexOf(prefix, startIndex, comparison);
            if (idxPrefix == -1)
                return false;

            var idxSuffix = text.IndexOf(prefix, idxPrefix, comparison);
            if (idxSuffix == -1)
                return false;

            enclosed = text.Substring(idxPrefix + 1, idxSuffix - idxPrefix);
            return true;
        }
    }
}