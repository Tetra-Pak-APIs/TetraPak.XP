

using System;
using System.Collections.Generic;
using System.Text;

namespace TetraPak.Auth.Xamarin.common
{
    // todo Consider moving StringExtensions to a common NuGet package to be referenced instead
    /// <summary>
    ///   Extends <see cref="string"/> objects and operations.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///   Splits a camel-case formatted string into its separate elements
        ///   (e.g. "CamelCase" => "Camel Case" or "helloWorld" => "hello World") 
        /// </summary>
        /// <param name="camelCaseValue">
        ///   The string to be split. 
        /// </param>
        /// <returns>
        ///   A <see cref="string"/>, split into its separate elements.
        /// </returns>
        public static string SplitCamelCase(this string camelCaseValue)
        {
            if (string.IsNullOrWhiteSpace(camelCaseValue))
                return camelCaseValue;

            var ca = camelCaseValue.ToCharArray();
            var sb = new StringBuilder();
            sb.Append(ca[0]);
            for (int i = 1; i < ca.Length; i++)
            {
                var c = ca[i];
                if (char.IsUpper(c))
                    sb.Append(' ');

                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        ///   Returns a <see cref="string"/> that is guaranteed to end with a specified suffix.
        /// </summary>
        /// <param name="self">
        ///   The original string.
        /// </param>
        /// <param name="suffix">
        ///   A <see cref="string"/> that will be appended to <paramref name="self"/> if not already present.
        /// </param>
        /// <param name="comparison">
        ///   (optional; default = <see cref="StringComparison.Ordinal"/>)<br/>
        ///   Used when detecting whether <paramref name="self"/> is already suffixed with the <paramref name="suffix"/> value.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> that is identical to<paramref name="self"/>, suffixed with <paramref name="suffix"/>.
        /// </returns>
        public static string EnsureEndsWith(this string self, string suffix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (self.EndsWith(suffix, comparison))
                return self;

            var sb = new StringBuilder(self);
            sb.Append(char.IsUpper(self[self.Length - 1]) ? suffix.ToUpper() : suffix);
            return sb.ToString();
        }

        /// <summary>
        ///   Returns a <see cref="string"/> that is guaranteed to not end with a specified suffix.
        /// </summary>
        /// <param name="self">
        ///   The original string.
        /// </param>
        /// <param name="suffix">
        ///   A <see cref="string"/> that will be removed if <paramref name="suffix"/> is found as suffix to <paramref name="self"/>.
        /// </param>
        /// <param name="comparison">
        ///   (optional; default = <see cref="StringComparison.Ordinal"/>)<br/>
        ///   Used when detecting whether <paramref name="self"/> is suffixed with the <paramref name="suffix"/> value.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> that is guaranteed to not end with <paramref name="suffix"/>.
        /// </returns>
        public static string EnsureNotEndsWith(this string self, string suffix, StringComparison comparison = StringComparison.Ordinal)
        {
            return self.EndsWith(suffix, comparison) 
                ? self.Substring(0, self.Length - suffix.Length) 
                : self;
        }
    }
}
