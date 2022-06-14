using System.Text;
using System.Text.RegularExpressions;

namespace TetraPak.XP
{
    /// <summary>
    ///   Provides convenient helper methods for working with <see cref="Regex"/> objects and expressions. 
    /// </summary>
    public static class RegexHelper
    {
        /// <summary>
        ///   Attempts converting wildcards '?' (any character) and '*' (any character, any number of times)
        ///   in the string into a .NET regex expression.
        /// </summary>
        /// <param name="text">
        ///   The <see cref="string"/> pattern, containing wildcards.
        /// </param>
        /// <param name="options">
        ///   (optional)<br/>
        ///   <see cref="RegexOptions"/> used for constructing the <see cref="Regex"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="Regex"/> object.
        /// </returns>
        public static Regex ToRegexFromWildcards(
            this string? text, 
            RegexOptions options = RegexOptions.None)
        {
            var ca = text.ThrowIfUnassigned(nameof(text)).ToCharArray();
            var sb = new StringBuilder();
            var last = (char)0;
            for (var i = 0; i < ca.Length; i++)
            {
                var c = ca[i];
                switch (c)
                {
                    case '*' when last != '\\':
                        sb.Append(".*");
                        break;
                    case '*' when last == '\\':
                        sb.Append("\\*");
                        break;
                    case '?' when last != '\\':
                        sb.Append('.');
                        break;
                    case '?' when last == '\\':
                        sb.Append("\\?");
                        break;
                    case '.':
                        sb.Append("\\.");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }

                last = c;
            }

            return new Regex(sb.ToString(), options);
        }
    }
}