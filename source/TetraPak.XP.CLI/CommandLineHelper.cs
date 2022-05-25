using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TetraPak.XP.CLI
{
    /// <summary>
    ///   Provides convenient helper methods for working with command line (arguments).  
    /// </summary>
    public static class CommandLineHelper
    {
        /// <summary>
        ///   Examines a collection of strings (eg. command line), looking for one or more specified
        ///   keys and, if found, returns the value that follows.   
        /// </summary>
        /// <param name="args">
        ///   The extended (read-only) list of strings.
        /// ,0
        /// </param>
        /// <param name="value">
        ///   Passes back the value succeeding the matching key in the list of strings.
        /// </param>
        /// <param name="keys">
        ///   One or more <see cref="string"/>s to look for.
        /// </param>
        /// <returns>
        ///   <c>true</c> if one of the <paramref name="keys"/> could be found and another <see cref="string"/>
        ///   succeeded it; otherwise <c>false</c>.
        /// </returns>
        public static bool TryGetValue(
            this IEnumerable<string> args,
#if NET5_0_OR_GREATER            
            [NotNullWhen(true)]
#endif            
            out string? value,
            params string[] keys)
        {
            var array = args.ToArray();
            for (var i = 0; i < array.Length - 1; i++)
            {
                if (!keys.Any(key => key.Equals(array[i], StringComparison.Ordinal)))
                    continue;

                value = array[i + 1];
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        ///   Examines a collection of strings (eg. command line) and returns a value indicating a match.
        /// </summary>
        /// <param name="args">
        ///   The extended (read-only) list of strings. 
        /// </param>
        /// <param name="keys">
        ///   One or more <see cref="string"/>s to look for.
        /// </param>
        /// <returns>
        ///   <c>true</c> if one of the <paramref name="keys"/> could be found; otherwise <c>false</c>.
        /// </returns>
        public static bool TryGetFlag(this IEnumerable<string> args, params string[] keys)
        {
            var array = args.ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                if (keys.Any(key => key.Equals(array[i], StringComparison.Ordinal)))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///   Examines a collection of strings, attempting to pass back the first item.
        /// </summary>
        /// <param name="args">
        ///   The extended (read-only) list of strings. 
        /// </param>
        /// <param name="value">
        ///   Passes back the first item in the collection if it exist.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the collection contains at least one item; otherwise <c>false</c>.
        /// </returns>
        public static bool TryGetFirstValue(
            this IEnumerable<string> args,
#if NET5_0_OR_GREATER            
            [NotNullWhen(true)]
#endif            
            out string? value)
        {
            var array = args.ToArray();
            value = array.Length >= 1 ? array[0] : null;
            return value is { };
        }
    }
}