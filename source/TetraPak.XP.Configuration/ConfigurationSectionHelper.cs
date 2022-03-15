using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace TetraPak.XP.Configuration
{
    public static class ConfigurationSectionHelper
    {
        public static T? Get<T>(
            this IConfiguration conf,
            T? useDefault = default,
            [CallerMemberName] string? caller = null,
            bool getDerived = false)
            =>
            conf.GetNamed(caller!, useDefault, getDerived);

        static T? getDerived<T>(
            this ConfigurationSectionDecorator conf,
            string key,
            T? useDefault = default)
        {
            return conf.Parent is { }
                ? conf.Parent.GetNamed(key, useDefault)
                : useDefault;
        }

        public static T? GetNamed<T>(
            this IConfiguration conf,
            string key,
            T useDefault = default!,
            bool getDerived = false,
            TypedValueParser<T>? typedValueParser = null)
        {
            var path = new ConfigPath(key);
            if (path.Count != 1)
            {
                // obtain from child entity ...
                var section = conf.GetSection(path.Root);
                key = path.Pop(1, SequentialPosition.Start);
                return section.GetNamed(key, useDefault, getDerived, typedValueParser);
            }

            var valueParsers = Configure.GetValueParsers(); 
            var delegates = Configure.GetValueDelegates();
            if (delegates.Any())
            {
                var args = new ConfigurationValueArgs<T>(conf, key, useDefault, valueParsers, null);
                foreach (var valueDelegate in delegates)
                {
                    var outcome = valueDelegate.GetValue(args);
                    if (outcome)
                        return outcome.Value!;
                }
            }

            var stringValue = conf[key];
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                if (getDerived && conf is ConfigurationSectionDecorator decorator)
                    return decorator.getDerived(key, useDefault);

                return useDefault;
            }

            if (typedValueParser is { } && typedValueParser(stringValue, out var value))
                return value;

            foreach (var valueParser in valueParsers)
            {
                if (valueParser(stringValue, typeof(T), out var obj, null!) && obj is T tValue)
                    return tValue;
            }

            return useDefault;
        }

        public static T? GetFromRoot<T>(
            this IConfiguration conf,
            T useDefault = default!,
            TypedValueParser<T>? parser = null,
            [CallerMemberName] string key = null!)
        {
            if (conf is not ConfigurationSectionDecorator wrapper) 
                return conf.GetNamed(key, useDefault, false, parser);
            
            if (wrapper.Parent is { })
                return wrapper.Parent.GetFromRoot(useDefault, parser, key);
                    
            return conf.GetNamed(key, useDefault, false, parser);
        }

        public static T? GetFromFieldThenSection<T>(
            this IConfiguration conf,
            T useDefault = default!,
            TypedValueParser<T>? parser = null,
            bool inherited = true,
            [CallerMemberName] string propertyName = null!)
        {
            return conf.TryGetFieldValue<T>(propertyName, out var fieldValue, inherited)
                ? fieldValue
                : conf.GetNamed<T>(propertyName);
        }

        internal static bool TryGetFieldValue<T>(
            this object obj,
            string propertyName,
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out T value,
            bool inherited = false)
        {
            value = default!;
            var fieldName = $"_{propertyName.ToLowerInitial()}";
            var field = obj.GetField(fieldName, inherited);
            var o = field?.GetValue(obj);
            if (o is not T tValue)
                return false;

            value = tValue;
            return true;
        }

        internal static FieldInfo? GetField(this object obj, string fieldName, bool inherited = false)
        {
            const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var fieldInfo = obj.GetType().GetField(fieldName, Flags);
            if (fieldInfo is { } || !inherited)
                return fieldInfo;

            var type = obj.GetType().BaseType;
            while (type is { } && fieldInfo is null)
            {
                fieldInfo = type.GetField(fieldName, Flags);
                type = type.BaseType;
            }

            return fieldInfo;
        }

        public static IEnumerable<IConfigurationSection> GetSubSections(this IConfiguration conf)
        {
            var nisse = conf.GetChildren(); 
            return conf.GetChildren().Where(i => i.IsConfigurationSection());
        }

        public static IConfigurationSection? GetSubSection(this IConfiguration conf, string key)
        {
            var path = new ConfigPath(key);
            if (path.Count == 1)
                return conf.GetSubSections().FirstOrDefault(i => i.Key == key);

            var section = conf.GetSubSection(path.Items[0]);
            return section?.GetSubSection(path.Pop(1, SequentialPosition.Start));
        }

        public static IConfigurationSection GetRequiredSubSection(this IConfiguration conf, string key)
        {
            var section = conf.GetSubSections().FirstOrDefault(i => i.Key == key);
            return section 
                   ??
                   throw new ArgumentOutOfRangeException(nameof(key), $"Configuration section not found: {key}");
        }

        public static bool IsConfigurationSection(this IConfigurationSection section)
        {
            return section is ConfigurationSectionDecorator || section.Value is null;
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