using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#nullable enable

namespace TetraPak.XP.DynamicEntities
{
    /// <summary>
    ///   Provides convenient helper methods for working with <see cref="DynamicPath"/>.
    /// </summary>
    public static class DynamicPathHelper
    {
        static readonly Regex s_pathRegex = new Regex(@"\{(?<ident>[_a-zA-Z]+)(?<optional>\?)?(?<default>\=[-\w]+)?\}",
            RegexOptions.Compiled | RegexOptions.Singleline);
        static readonly Regex s_pathRegexIgnoreCase = new Regex(@"\{(?<ident>[_a-zA-Z]+)(?<optional>\?)?(?<default>\=[-\w]+)?\}",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        /// <summary>
        ///   Creates and returns a new <see cref="DynamicPath"/> where the leading elements are removed.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="DynamicPath"/> to be modified.
        /// </param>
        /// <param name="path">
        ///   The leading elements to be removed.
        /// </param>
        /// <param name="comparison">
        ///   (optional)<br/>
        ///   When set; specifies how elements are compared during validation.
        ///   When unassigned, no string matching will take place: The new <see cref="DynamicPath"/> will simply
        ///   be trimmed by the number of elements found in <paramref name="path"/>. 
        /// </param>
        /// <returns>
        ///   A new <see cref="DynamicPath"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   <paramref name="path"/> does not match the leading elements of <see cref="DynamicPath"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <see cref="path"/> is longer than the <see cref="DynamicPath"/> to be trimmed.
        /// </exception>
        public static DynamicPath TrimLeading(
            this DynamicPath self, 
            DynamicPath path, 
            StringComparison? comparison = null)
        {
            var selfStack = self.GetStack().ToArray();
            var pathStack = path.GetStack().ToArray();
            if (pathStack.Length > selfStack.Length)
                throw new ArgumentOutOfRangeException(nameof(path), "Leading path is too long");
            
            var newStack = new string[selfStack.Length - pathStack.Length];
            if (newStack.Length == 0)
                return new DynamicPath();

            if (comparison is null)
                return self.Pop(pathStack.Length, SequentialPosition.Start);
            
            for (var i = 0; i < pathStack.Length; i++)
            {
                if (!string.Equals(selfStack[i], pathStack[i], comparison.Value))
                    throw new InvalidOperationException($"Leading elements not found: {path}");
            }
            selfStack.CopyTo(newStack, pathStack.Length+1);
            return new DynamicPath(newStack);
        }

        /// <summary>
        ///   Examines and substitutes variable elements with specified values. 
        /// </summary>
        /// <param name="self">
        ///   The <see cref="DynamicPath"/>. 
        /// </param>
        /// <param name="values">
        ///   Provides values for substituted elements.
        /// </param>
        /// <param name="ignoreCase">
        ///   (optional; default=<c>true</c>)<br/>
        ///   Specifies whether to ignore case when matching variable element identifiers.  
        /// </param>
        /// <returns>
        ///   A <see cref="DynamicPath"/> with variable elements substituted.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///   Dynamic path substitution requires that the variable elements of the dynamic path
        ///   are qualified between curly brackets ('{' and '}'), like in this example:
        ///   <c>/one/{two}/{three=Hello}/{four?}</c>. In that path the second (<c>{two}</c>), third ({three=Hello})
        ///   and fourth ({four?}) elements are variable elements.</para>
        ///   <para>
        ///   The second element (<c>{two}</c>) is simply substituted with the value found in the
        ///   <paramref name="values"/>'s "Two" property. If <paramref name="values"/> does not declare
        ///   a "Two" property, or its value is <c>null</c> or empty
        ///   a <see cref="InvalidOperationException"/> is thrown.</para>
        ///   <para>
        ///   The third element specifies a default value ("Hello"), to be used when
        ///   resolving the substitute value fails or results in a <c>null</c>/empty value.
        ///   </para> 
        ///   <para>
        ///   The fourth element ({four?}) is optional (qualified buy the '?' operator). This element is treated
        ///   like the second (<c>{two}</c>) element but will not result in a exception if no value could
        ///   be resolved from <paramref name="values"/>. For example, calling the method like so ...</para>
        ///   <code>
        ///   var values = new { Two = "second" };
        ///   var path = ((DynamicPath)"/one/{two}/{three?}/{four=Hello}").Substitute(new { Two = "second" });
        ///   </code>
        ///   ... will return this dynamic path: "<c>/one/second/Hello</c>".
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   A non-optional variable element name has no corresponding property in <paramref name="values"/>.  
        /// </exception>
        public static DynamicPath Substitute(this DynamicPath self, object values, bool ignoreCase = true)
        {
            var props = values.GetType().GetProperties();
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!props.Any())
                return self;

            var items = new List<string>();
            foreach (var element in self)
            {
                if (!isVariable(element, out var isOptional, out var defaultValue))
                {
                    items.Add(element);
                    continue;
                }

                if (tryGetElementValue(element, defaultValue, out var value))
                {
                    items.Add(value!);
                    continue;
                }
                
                if (isOptional)
                    continue;

                throw new InvalidOperationException($"No value found for variable element \"{element}\"");
            }

            return new DynamicPath(items.ToArray());

            bool isVariable(string s, out bool isOptional, out string? defaultValue)
            {
                var match = ignoreCase
                    ? s_pathRegexIgnoreCase.Match(s)
                    : s_pathRegex.Match(s);

                if (!match.Success)
                {
                    isOptional = false;
                    defaultValue = null!;
                    return false;
                }

                isOptional = match.Groups["optional"].Success;
                defaultValue = match.Groups["default"].Value;
                return true;
            }

            bool tryGetElementValue(string identifier, string? defaultValue, out string? value)
            {
                var propertyName = identifier.Trim('{', '}');
                var prop = props.FirstOrDefault(pi => pi.Name.Equals(propertyName, comparison));
                if (prop is null || !prop.CanRead)
                {
                    if (!string.IsNullOrEmpty(defaultValue))
                    {
                        value = defaultValue;
                        return true;
                    }
                    value = null;
                    return false;
                }
                
                var obj = prop.GetValue(values); 
                value = null;
                switch (obj)
                {
                    case null:
                    case string stringValue when string.IsNullOrEmpty(stringValue):
                        return false;
                    case string stringValue:
                        value = stringValue;
                        return true;
                    default:
                        value = obj.SafeToString(null!);
                        return !string.IsNullOrEmpty(value);
                }
            }
        }
        
    }
}