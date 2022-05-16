using System;

namespace TetraPak.XP
{
    public static class ValidationHelper
    {
        public static T ThrowIfNull<T>(this T value, string paramName)
        {
            if (value is null)
                throw new ArgumentNullException(paramName);

            return value;
        }
        
        public static string ThrowIfUnassigned(this string? value, string paramName, bool isWhitespaceAllowed = false)
        {
            if (value.IsUnassigned())
                throw new ArgumentNullException(paramName);

            return value!;
        }
    }
}