using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Diagnostics
{
    public static class LogHelper
    {
          /// <summary>
        ///   Generates and returns a string to reflect the state of an object.
        /// </summary>
        /// <param name="obj">
        ///     The object.
        /// </param>
        /// <param name="context">
        ///     Specifies how to render state from the specified object. 
        /// </param>
        /// <returns>
        ///   A JSON formatted string, reflecting the object's current state.
        /// </returns>
        /// <seealso cref="RestrictedValueAttribute"/>
        public static async Task GetStateDumpAsync(this object obj, StateDumpContext context)
        {
            await renderObjectStateAsync(obj);
            return;

            async Task renderObjectStateAsync(object o)
            {
                if (o is StackTrace)
                {
                    context.AppendLine("\"");
                    context.AppendLine($"{o}\"");
                    return;
                }

                context.AppendLine("{");
                context.Indent();
                var propertyInfos = o.GetType().GetProperties().ToArray();
                var lastSeparatorIndex = -1;
                for (var i = 0; i < propertyInfos.Length; i++)
                {
                    var pi = propertyInfos[i];
                    if (!isIncluded(pi, out var value))
                        continue;

                    context.Append(context.Indentation);
                    context.Append('"');
                    context.Append(pi.Name);
                    context.Append('"');
                    context.Append(": ");
                    var restricted = pi.GetCustomAttribute<RestrictedValueAttribute>();
                    var isRestricted = restricted is { };
                    if (restricted is { } && context.Log is { })
                    {
                        isRestricted = !restricted.IsDisclosedForLog(context.Log);
                    }

                    value = isRestricted
                        ? getRestrictedJsonValue(pi, o)
                        : getJsonValue(pi, o);

                    context.Append(value);
                    lastSeparatorIndex = context.Length;
                    context.AppendLine(",");
                }

                if (lastSeparatorIndex != -1 && !await appendAttachedAsync(o) && !context.IsRetainingTrailingSeparator)  
                {
                    context.Remove(lastSeparatorIndex, 1); // remove final separator (',')
                }
                context.IsRetainingTrailingSeparator = false;
                context.Outdent();
                context.Append(context.Indentation);
                context.AppendLine("}");
            }

            string getJsonValue(PropertyInfo propertyInfo, object o)
            {
                var value = propertyInfo.GetValue(o);
                if (value is null)
                    return "null";

                var result = value is bool || value.IsNumeric() && !propertyInfo.PropertyType.IsEnum
                    ? value.ToString()?.ToLower()
                    : $"\"{value}\"";
                return result ?? "null";
            }

            string getRestrictedJsonValue(PropertyInfo propertyInfo, object o)
            {
                var value = propertyInfo.GetValue(o);
                return value is null
                    ? "null"
                    : "\"[*** RESTRICTED ***]\"";
            }

            async Task<bool> appendAttachedAsync(object source)
            {
                if (context.AttachedStateDumpsHandlers is null)
                    return false;

                var wasAttached = false;
                foreach (var handler in context.AttachedStateDumpsHandlers)
                {
                    wasAttached = wasAttached || await handler(source, /*sb, */context);
                }

                return wasAttached;
            }

            bool isIncluded(PropertyInfo propertyInfo, out string? value)
            {
                // todo This is a good place to support potential custom handler(s) to decide whether a property is to be included in a state dump
                value = null;
                return propertyInfo.GetCustomAttribute<StateDumpAttribute>() is { };
            }
        }
          
          /// <summary>
          ///   Examines the <see cref="RestrictedValueAttribute"/> and returns a value indicating whether
          ///   the decorated value can be disclosed for a specified <see cref="ILog"/>.
          /// </summary>
          /// <param name="self">
          ///   The attribute.
          /// </param>
          /// <param name="log">
          ///   The intended logger provider.
          /// </param>
          /// <returns>
          ///   <c>true</c> if the decorated value can be disclosed for <paramref name="log"/>;
          ///   otherwise <c>false</c>. 
          /// </returns>
          public static bool IsDisclosedForLog(this RestrictedValueAttribute self, ILog log)
          {
              if (self.DisclosureLogLevel == LogRank.None)
                  return false;

              return log.IsEnabled(self.DisclosureLogLevel);
          }
    }
}