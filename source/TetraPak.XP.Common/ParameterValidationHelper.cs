using System;

namespace TetraPak.XP
{
    /// <summary>
    ///   Provides convenient helper methods for working with method parameters. 
    /// </summary>
    public static class ParameterValidationHelper
    {
        /// <summary>
        ///   Examines the value of a named parameter and throws an <see cref="ArgumentNullException"/>
        ///   if it is <c>null</c>.
        /// </summary>
        /// <param name="value">
        ///   The parameter value.
        /// </param>
        /// <param name="paramName">
        ///   The parameter name.
        /// </param>
        /// <typeparam name="T">
        ///   The parameter <see cref="Type"/>.
        /// </typeparam>
        /// <returns>
        ///   The parameter <paramref name="value"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   The parameter <paramref name="value"/> was <c>null</c>.
        /// </exception>
        /// <seealso cref="ThrowIfUnassigned"/>
        public static T ThrowIfNull<T>(this T value, string paramName)
        {
            if (value is null)
                throw new ArgumentNullException(paramName);

            return value;
        }

        /// <summary>
        ///   Examines the value of a named <see cref="string"/> parameter and throws an
        ///   <see cref="ArgumentNullException"/> if it is unassigned.
        /// </summary>
        /// <param name="value">
        ///   The <see cref="string"/> parameter value to be examined.
        /// </param>
        /// <param name="paramName">
        ///   The parameter name.
        /// </param>
        /// <param name="allowWhitespace">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies whether a string with only whitespace is considered to be assigned.
        /// </param>
        /// <returns>
        ///   The parameter <paramref name="value"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   The parameter <see cref="string"/> <paramref name="value"/> was <c>null</c>.
        /// </exception>
        /// <remarks>
        ///   The method invokes the <see cref="StringHelper.IsUnassigned"/> extension method
        ///   to resolve whether <paramref name="value"/> is unassigned.
        /// </remarks>
        /// <seealso cref="StringHelper.IsUnassigned"/>
        public static string ThrowIfUnassigned(this string? value, string paramName, bool allowWhitespace = false)
        {
            if (value.IsUnassigned(allowWhitespace))
                throw new ArgumentNullException(paramName);

            return value!;
        }
    }
}