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
    /// <summary>
    ///   Provides convenient helper methods for working with the configuration framework. 
    /// </summary>
    public static class ConfigurationSectionHelper
    {
        /// <summary>
        ///   Gets a typed value, using the caller's name as the value key to invoke <see cref="GetNamed{T}"/>.
        /// </summary>
        /// <param name="conf">
        ///     The extended configuration instance.
        /// </param>
        /// <param name="useDefault">
        ///   (optional; default=&lt;default of specified type (<typeparamref name="T"/>)&gt;)<br/>
        ///   A custom default value to be returned if the configuration does not support the value key.  
        /// </param>
        /// <param name="getDerived">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Set this to automatically fall back to the configuration's parent configuration
        ///   (such as the enclosing section), when available, and resolve the value from it.
        /// </param>
        /// <param name="caller">
        ///   (optional; default=The caller's (property or method) name)<br/>
        ///   This should not be specified. Doing so is akin to just invoking <see cref="GetNamed{T}"/>.
        /// </param>
        /// <typeparam name="T">
        ///   The requested value type.
        /// </typeparam>
        /// <returns>
        ///   The resolved value if successfully resolved; otherwise <paramref name="useDefault"/>. 
        /// </returns>
        /// <seealso cref="GetNamed{T}"/>
        public static T? Get<T>(
            this IConfiguration conf,
            T? useDefault = default,
            bool getDerived = false,
            [CallerMemberName] string? caller = null)
            =>
                conf.GetNamed(caller!, useDefault, getDerived);

        /// <summary>
        ///   Sets (overrides) a specified value.
        /// </summary>
        /// <param name="conf">
        ///   The extended configuration instance.
        /// </param>
        /// <param name="value">
        ///   The value to be assigned.
        /// </param>
        /// <param name="caller">
        ///   (optional; default=The caller's (property or method) name)<br/>
        ///   This should not be specified. Doing so is akin to just invoking <see cref="SetNamed"/>.
        /// </param>
        public static void Set(
            this IConfiguration conf,
            object? value,
            [CallerMemberName] string? caller = null)
            =>
                conf.SetNamed(caller!, value);

        static T? getDerived<T>(
            this ConfigurationSectionDecorator conf,
            string key,
            T? useDefault = default)
        {
            return conf.Parent is { }
                ? conf.Parent.GetNamed(key, useDefault)
                : useDefault;
        }

        /// <summary>
        ///   Gets a named configuration value.
        /// </summary>
        /// <param name="conf">
        ///   The extended configuration instance.
        /// </param>
        /// <param name="key">
        ///   Identifies the requested value. 
        /// </param>
        /// <param name="useDefault">
        ///   (optional; default=&lt;default of specified type (<typeparamref name="T"/>)&gt;)<br/>
        ///   A custom default value to be returned if the configuration does not support the value key.  
        /// </param>
        /// <param name="getDerived">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Set this to automatically fall back to the configuration's parent configuration
        ///   (such as the enclosing section), when available, and resolve the value from it.
        /// </param>
        /// <param name="valueParser">
        ///   (optional)<br/>
        ///   A (custom) value parser to be invoked to automatically parse a string
        ///   representation of the requested value. 
        /// </param>
        /// <typeparam name="T">
        ///   The requested value type.
        /// </typeparam>
        /// <returns>
        ///   The resolved value if successfully resolved; otherwise <paramref name="useDefault"/>. 
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="conf"/> or <paramref name="key"/> was unassigned.
        /// </exception>
        /// <seealso cref="Set"/>
        public static T? GetNamed<T>(
            this IConfiguration conf,
            string key,
            T? useDefault = default,
            bool getDerived = false,
            TypedValueParser<T>? valueParser = null)
        {
            var path = new ConfigPath(key.ThrowIfUnassigned(nameof(key)));
            if (path.Count != 1)
            {
                // obtain from child entity ...
                var section = conf.ThrowIfNull(nameof(conf)).GetSection(path.Root);
                key = path.Pop(1, SequentialPosition.Start);
                return section.GetNamed(key, useDefault, getDerived, valueParser);
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

            string? stringValue = null;
            var decorator = conf as ConfigurationSectionDecorator;
            if (decorator is { } && (decorator.Overrides?.TryGetValue(key, out var oValue) ?? false))
            {
                if (oValue is T otValue)
                    return otValue;

                stringValue = oValue as string ?? null;
            }

            stringValue ??= conf[key];
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                if (getDerived && decorator is {})
                    return decorator.getDerived(key, useDefault);

                return useDefault;
            }

            if (valueParser is { } && valueParser(stringValue, out var value))
                return value;

            foreach (var parser in valueParsers)
            {
                if (parser(stringValue, typeof(T), out var obj, null!) && obj is T tValue)
                    return tValue;
            }

            return useDefault;
        }
        
        /// <summary>
        ///   Sets or overrides a named value (when extended configuration supports this feature).
        /// </summary>
        /// <param name="conf">
        ///   The extended <see cref="IConfiguration"/>.
        /// </param>
        /// <param name="key">
        ///   Identifies the value. 
        /// </param>
        /// <param name="value">
        ///   The value to be used.
        /// </param>
        public static void SetNamed(this IConfiguration conf, string key, object? value)
        {
            key.ThrowIfUnassigned(nameof(key));
            if (conf is not ConfigurationSectionDecorator decorator) 
                return;
            
            decorator.Overrides ??= new Dictionary<string, object?>();
            decorator.Overrides[key] = value;
        }
        
        /// <summary>
        ///   Removes an overridden value (when extended configuration supports this feature),
        ///   possibly resetting it to the value specified in the original configuration source.
        /// </summary>
        /// <param name="conf">
        ///   The extended <see cref="IConfiguration"/>.
        /// </param>
        /// <param name="key">
        ///   Identifies the value to be cleared. 
        /// </param>
        public static void ClearNamed(this IConfiguration conf, string key)
        {
            if (conf is ConfigurationSectionDecorator { Overrides: {} } decorator 
                && decorator.Overrides.ContainsKey(key.ThrowIfUnassigned(nameof(key))))
            {
                decorator.Overrides.Remove(key);
            }
        }

        /// <summary>
        ///   Retrieves a configured value from the root of a hierarchical configuration tree.
        /// </summary>
        /// <param name="conf">
        ///     The extended configuration instance.
        /// </param>
        /// <param name="parser"></param>
        /// <param name="useDefault">
        ///   (optional; default=&lt;default of specified type (<typeparamref name="T"/>)&gt;)<br/>
        ///   A custom default value to be returned if the configuration does not support the value key.  
        /// </param>
        /// <param name="caller">
        ///   (optional; default=The caller's (property or method) name)<br/>
        ///   This should not be specified.
        /// </param>
        /// <typeparam name="T">
        ///   The requested value type.
        /// </typeparam>
        /// <returns>
        ///   The resolved value if successfully resolved; otherwise <paramref name="useDefault"/>. 
        /// </returns>
        public static T? GetFromRoot<T>(
            this IConfiguration conf,
            T? useDefault = default,
            TypedValueParser<T>? parser = null,
            [CallerMemberName] string caller = null!)
        {
            if (conf is not ConfigurationSectionDecorator wrapper) 
                return conf.GetNamed(caller, useDefault, false, parser);
            
            return wrapper.Parent is { } 
                ? wrapper.Parent.GetFromRoot(useDefault, parser, caller) 
                : conf.GetNamed(caller, useDefault, false, parser);
        }

        /// <summary>
        ///   Intended to be invoked from property getter, this method retrieves a configuration value by first looking
        ///   for an internal field and (see remarks), if that fails, fall back to a configured value.
        /// </summary>
        /// <param name="conf">
        ///   The extended configuration instance.
        /// </param>
        /// <param name="valueParser">
        ///   (optional)<br/>
        ///   A (custom) value parser to be invoked to automatically parse a string
        ///   representation of the requested value. 
        /// </param>
        /// <param name="useDefault">
        ///   (optional; default=&lt;default of specified type (<typeparamref name="T"/>)&gt;)<br/>
        ///   A custom default value to be returned if the configuration does not support the value key.  
        /// </param>
        /// <param name="getDerived">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Set this to automatically fall back to the configuration's parent configuration
        ///   (such as the enclosing section), when available, and resolve the value from it.
        /// </param>
        /// <param name="caller">
        ///   (optional; default=The caller's [property getter] name)<br/>
        ///   This should not be specified.
        /// </param>
        /// <typeparam name="T">
        ///   The requested value type.
        /// </typeparam>
        /// <returns>
        ///   The resolved value if successfully resolved; otherwise <paramref name="useDefault"/>. 
        /// </returns>
        /// <remarks>
        ///   When trying to resolve the value by looking for an (internal) instance field, the naming convention
        ///   is: [<paramref name="caller"/> with lower leading character and prefixed by '_']. When calling this method from
        ///   the getter of a property called "StringValue" the field is assumed to be named "_stringValue".
        /// </remarks>
        public static T? GetFromFieldThenSection<T>(
            this IConfiguration conf,
            T useDefault = default!,
            TypedValueParser<T>? valueParser = null,
            bool getDerived = true,
            [CallerMemberName] string caller = null!)
        {
            return conf.TryGetFieldValue<T>(caller, out var fieldValue, getDerived)
                ? fieldValue
                : conf.GetNamed<T>(caller);
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

        /// <summary>
        ///   Retrieves all sub sections of a configuration graph . 
        /// </summary>
        /// <param name="conf">
        ///     The extended configuration instance.
        /// </param> 
        /// <returns>
        ///   A collection of <see cref="IConfigurationSection"/> (empty if none exists).
        /// </returns>
        public static IEnumerable<IConfigurationSection> GetSubSections(this IConfiguration conf)
        {
            return conf.GetChildren().Where(i => i.IsConfigurationSection());
        }

        /// <summary>
        ///   Gets a specified configuration sub section, if available.
        /// </summary>
        /// <param name="conf">
        ///   The extended configuration instance.
        /// </param>
        /// <param name="key">
        ///   Identifies the requested sub section.
        /// </param>
        /// <returns>
        ///   A <see cref="IsConfigurationSection"/> object representing the named configuration if it
        ///   could be successfully resolved; otherwise <c>null</c>.
        /// </returns>
        /// <seealso cref="GetRequiredSubSection"/>
        public static IConfigurationSection? GetSubSection(this IConfiguration conf, string key)
        {
            var path = new ConfigPath(key);
            if (path.Count == 1)
            {
                var subSection = conf.GetSubSections().FirstOrDefault(i => i.Key == key);
                return subSection is ConfigurationSectionDecorator decorator 
                    ? decorator.GetSection() 
                    : subSection;
            }

            var section = conf.GetSubSection(path.Items[0]);
            return section?.GetSubSection(path.Pop(1, SequentialPosition.Start));
        }

        /// <summary>
        ///   Gets a specified configuration sub section,
        ///   or throws an <see cref="ArgumentOutOfRangeException"/> otherwise.
        /// </summary>
        /// <param name="conf">
        ///   The extended configuration instance.
        /// </param>
        /// <param name="key">
        ///   Identifies the requested sub section.
        /// </param>
        /// <returns>
        ///   A <see cref="IsConfigurationSection"/> object representing the named configuration if it
        ///   could be successfully resolved; otherwise a <see cref="ArgumentNullException"/>.
        /// </returns>
        /// <seealso cref="GetRequiredSubSection"/>
        public static IConfigurationSection GetRequiredSubSection(this IConfiguration conf, string key)
        {
            var section = conf.GetSubSections().FirstOrDefault(i => i.Key == key);
            return section 
                   ??
                   throw new ArgumentOutOfRangeException(nameof(key), $"Configuration section not found: {key}");
        }

        /// <summary>
        ///   Yeah, this looks like a strange method doesn't it, checking whether a <see cref="IsConfigurationSection"/>
        ///   is a configuration section? See remarks for more details. 
        /// </summary>
        /// <param name="section">
        ///   The <see cref="IConfigurationSection"/> to be examined.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="section"/> represents a section (2 or more sub values);
        ///   <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        ///   The <see cref="IConfiguration"/> code framework returns all values, even the ones with a scalar value,
        ///   as <see cref="IsConfigurationSection"/> objects. This method checks to see it is actually a 'section'
        ///   (contains two or more sub values).
        /// /// </remarks>
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

            if (stringValue.TryParseTimeSpan(TimeUnits.Milliseconds, out timeSpan))
            {
                value = timeSpan;
                return true;
            }
            
            value = TimeSpan.Zero;
            return false;
        }

    }
}