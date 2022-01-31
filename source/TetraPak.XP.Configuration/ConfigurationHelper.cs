using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraPak.XP.Configuration
{
    public static class ConfigurationHelper
    {
        public static async Task<bool> ContainsKeyAsync(this IConfiguration configuration, string key, bool ignoreCase = false)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var children = await configuration.GetChildrenAsync();
            return ignoreCase 
                ? children.Any(i => i.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)) 
                : children.Any(i => i.Key == key);
        }

        public static async Task<bool> IsEmpty(this IConfiguration configuration) // todo rename => IsEmptyAsync
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            return !(await configuration.GetChildrenAsync()).Any();
        }
        
        internal static async Task<string> ToStringValues(this IConfiguration self, bool recursive = false, int indent = 0) // todo rename => ToStringValuesAsync
        {
            if (await self.IsEmpty())
                return string.Empty;

            var indentation = indent == 0 ? string.Empty : new string(' ', indent);
            var children = await self.GetChildrenAsync();
            var sb = new StringBuilder();
            foreach (var child in children)
            {
                sb.Append(indentation);
                sb.Append(child.Key);
                sb.Append(": ");
                if (child.Value is { })
                {
                    sb.AppendLine(child.Value);
                    continue;
                }

                if (!(await child.GetChildrenAsync()).Any())
                {
                    sb.AppendLine("(empty)");
                    continue;
                }

                if (!recursive)
                {
                    sb.AppendLine("{ ... }");
                    continue;
                }

                sb.AppendLine(" {");
                sb.AppendLine(await child.ToStringValues(true, indent + 3));
                sb.Append(indentation);
                sb.AppendLine("}");
            }

            return sb.ToString();
        }
        
                /// <summary>
        ///   Parses a <see cref="string"/> as a configured <see cref="bool"/> value. 
        /// </summary>
        /// <param name="stringValue">
        ///   The (configured) <see cref="bool"/> string representation.
        /// </param>
        /// <param name="value">
        ///   Passes back the parsed boolean value.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="stringValue"/> was successfully parsed; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        ///   A configured <see cref="bool"/> value accepts three forms:
        ///   <list type="bullet">
        ///     <item>
        ///       <term>true/false</term>
        ///       <description>
        ///       - Just use standard C# identifiers <c>true</c> or <c>false</c> (not case sensitive).
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>yes/no</term>
        ///       <description>
        ///       - Use plain English words <c>yes</c> or <c>no</c> for true/false (not case sensitive).
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>1/0</term>
        ///       <description>
        ///       - Use numbers <c>1</c> or <c>0</c> for true/false.
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        public static bool TryParseConfiguredBool(
            this string stringValue, 
            out bool value)
        {
            value = false;
            if (string.IsNullOrWhiteSpace(stringValue))
                return false;
            
            if (bool.TryParse(stringValue.ToLowerInvariant(), out value))
                return true;

            switch (stringValue)
            {
                case "1":
                    return true;
                case "0":
                    return false;
            }

            if (stringValue.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                value = true;
                return true;
            }

            if (stringValue.Equals("no", StringComparison.InvariantCultureIgnoreCase))
            {
                value = false;
                return true;
            }

            return false;
        }
                
        /// <summary>
        ///   Parses a <see cref="string"/> as a configured <see cref="TimeSpan"/> value. 
        /// </summary>
        /// <param name="stringValue">
        ///   The (configured) <see cref="TimeSpan"/> string representation.
        /// </param>
        /// <param name="value">
        ///   Passes back the parsed <see cref="TimeSpan"/> value.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="stringValue"/> was successfully parsed; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        ///   A configured <see cref="TimeSpan"/> value accepts two forms:
        ///   <list type="bullet">
        ///     <item>
        ///       <term>hh:mm:ss</term>
        ///       <description>
        ///       - Use standard C# syntax for <see cref="TimeSpan"/> string representation.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>seconds</term>
        ///       <description>
        ///       - Use integer value to express <see cref="TimeSpan"/> in seconds.
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        public static bool TryParseConfiguredTimeSpan(this string stringValue, out TimeSpan value) 
        {
            if (int.TryParse(stringValue, out var milliseconds))
            {
                value = TimeSpan.FromMilliseconds(milliseconds);
                return true;
            }

            if (TimeSpan.TryParse(stringValue, out var timeSpan))
            {
                value = timeSpan;
                return true;
            }

            value = TimeSpan.Zero;
            return false;
        }
    }
}