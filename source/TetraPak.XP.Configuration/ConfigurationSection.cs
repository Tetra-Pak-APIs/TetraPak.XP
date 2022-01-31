using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TetraPak.XP.Logging;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

#nullable enable

namespace TetraPak.XP.Configuration
{
    /// <summary>
    ///   Provides access to the configuration framework through a POCO class. 
    /// </summary>
    [DebuggerDisplay("{" + nameof(ConfigPath) + "}")]
    public abstract class ConfigurationSection 
    {
#if DEBUG
        static int s_debugCount;

        public int DebugInstanceId { get; } = s_debugCount++;
#endif

        static readonly List<ArbitraryValueParser> s_valueParsers = new();

        /// <summary>
        ///   Gets a value that indicates whether the configuration section contains no information. 
        /// </summary>
        [JsonIgnore] 
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IsEmpty { get; }

        /// <summary>
        ///   Can be overridden. Returns the expected configuration section identifier like in this example:<br/>
        ///   <code>
        ///   "MySection": {
        ///     :
        ///   }
        ///   </code>
        /// </summary>
        public string SectionIdentifier { get; protected set; }
        
        /// <summary>
        ///   Gets the encapsulated <see cref="IConfigurationSection"/>.  
        /// </summary>
        public IConfigurationSection? Section { get; protected set; }

        /// <summary>
        ///   Gets the parent <see cref="IConfiguration"/> section
        ///   (or <c>null</c> if this section is also the configuration root).
        /// </summary>
        public IConfiguration? ParentConfiguration { get; }
        
        /// <summary>
        ///   Gets a logger.
        /// </summary>
        public ILog? Log { get; protected set; }

        /// <summary>
        ///   Gets the section's configuration path.
        /// </summary>
        public ConfigPath? ConfigPath { get; protected set; }
        
        string? getSectionKey(ConfigPath? sectionIdentifier, IConfiguration? configuration)
        {
            if (sectionIdentifier?.IsEmpty() ?? true)
            {
                sectionIdentifier = SectionIdentifier;
            }
            if (sectionIdentifier.Count == 1)
                return sectionIdentifier;

            if (configuration is not IConfigurationSection section) 
                return sectionIdentifier;
            
            var sectionPath = (ConfigPath) section.Path;
            return sectionIdentifier.StringValue!.StartsWith(sectionPath!) 
                ? sectionIdentifier.Pop(sectionPath.Count, SequentialPosition.Start) 
                : sectionIdentifier;
        }

        /// <summary>
        ///   Obtains a <see cref="FieldInfo"/> object for a specified field.
        /// </summary>
        /// <param name="fieldName">
        ///   Identifies the requested field.
        /// </param>
        /// <param name="inherited">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies whether to look for the field in base type(s).
        /// </param>
        /// <returns>
        ///   A <see cref="FieldInfo"/> object.
        /// </returns>
        protected virtual FieldInfo? OnGetField(string fieldName, bool inherited = false)
        {
            const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var fieldInfo = GetType().GetField(fieldName, Flags);
            if (fieldInfo is { } || !inherited)
                return fieldInfo;

            var type = GetType().BaseType;
            while (type is { } && fieldInfo is null)
            {
                fieldInfo = type.GetField(fieldName, Flags);
                type = type.BaseType;
            }

            return fieldInfo;
        }
        
#if NET5_0_OR_GREATER
        internal bool TryGetFieldValue<T>(string propertyName, [NotNullWhen(true)] out T value, bool inherited = false)
#else
        internal bool TryGetFieldValue<T>(string propertyName, out T value, bool inherited = false)
#endif
        {
            value = default!;
            var fieldName = $"_{propertyName.ToLowerInitial()}";
            var field = OnGetField(fieldName, inherited);
            var o = field?.GetValue(this);
            if (o is not T tValue)
                return false;

            value = tValue;
            return true;
        }

        internal void SetFieldValue(string propertyName, object value)
        {
            var fieldName = $"_{propertyName.ToLowerInitial()}";
            var field = OnGetField(fieldName);
            field?.SetValue(this, value);
        }

        /// <summary>
        ///   Attempts reading a value, first from a backing field and then from the configuration section.
        /// </summary>
        /// <param name="useDefault">
        ///   A default value to be returned if no value could be obtained,
        ///   from a backing field or the configuration section. 
        /// </param>
        /// <param name="parser">
        ///   (optional)<br/>
        ///   A parser handler, allowing custom parsing of non-standard value types.
        /// </param>
        /// <param name="inherited">
        ///   (optional; default=<c>true</c>)<br/>
        ///   Specifies whether to include backing fields from super classes.
        /// </param>
        /// <param name="propertyName">
        ///   The name of the requested value (presumably a property name).
        /// </param>
        /// <remarks>
        ///   <para>
        ///   The method first reads a value from a backing field (name convention based on property).
        ///   If the field is <c>null</c> (or does not exist) the method instead attempts reading the value
        ///   from the configuration section. If the configuration section also doesn't supported the value
        ///   the method returns the <paramref name="useDefault"/> value.
        ///   </para>
        ///   <para>
        ///   For values that must be fetched from the configuration section the method automatically supports
        ///   parsing standard value types, such as <see cref="DateTime"/>, <see cref="TimeSpan"/>
        ///   and all the numeric value types. For other types you need to pass
        ///   a <paramref name="parser"/> delegate or the method will not be able to convert the textual value
        ///   found in the configuration section. 
        ///   </para> 
        /// </remarks>
        protected async Task<T?> GetFromFieldThenSectionAsync<T>(
            T useDefault = default!, 
            TypedValueParser<T>? parser = null, 
            bool inherited = true,
            [CallerMemberName] string propertyName = null!)
        {
            if (TryGetFieldValue<T>(propertyName, out var fieldValue, inherited))
                return fieldValue;

            var s = Section is {} ? await Section.GetAsync(propertyName) : null;
            return parser is {}
                ? parser(s, out var sectionValue) ? sectionValue : useDefault! 
                : await parseSectionValueAsync<T>(s, useDefault!, propertyName);
        }
        
        /// <summary>
        ///   Attempts reading a value, first from the configuration section and then from a backing field.
        /// </summary>
        /// <param name="useDefault">
        ///   A default value to be returned if no value could be obtained,
        ///   from a backing field or the configuration section. 
        /// </param>
        /// <param name="parser">
        ///   (optional)<br/>
        ///   A parser handler, allowing custom parsing of non-standard value types.
        /// </param>
        /// <param name="inherited">
        ///   (optional; default=<c>true</c>)<br/>
        ///   Specifies whether to include backing fields from super classes.
        /// </param>
        /// <param name="propertyName">
        ///   The name of the requested value (presumably a property name).
        /// </param>
        /// <remarks>
        ///   <para>
        ///   The method first reads a value from the configuration section (name convention based on property).
        ///   If the value is not found in the configuration section the method instead attempts reading the value
        ///   from a backing field. If the backing field does not exist or is unassigned 
        ///   the method returns the <paramref name="useDefault"/> value.
        ///   </para>
        ///   <para>
        ///   For values that can successfully be fetched from the configuration section the method automatically
        ///   supports parsing standard value types, such as <see cref="DateTime"/>, <see cref="TimeSpan"/>
        ///   and all the numeric value types. For other types you need to pass
        ///   a <paramref name="parser"/> delegate or the method will not be able to convert the textual value
        ///   found in the configuration section. 
        ///   </para> 
        /// </remarks>
        protected async Task<T?> GetFromSectionThenField<T>(
            T? useDefault = default, 
            TypedValueParser<T>? parser = null, 
            bool inherited = true,
            [CallerMemberName] string propertyName = null!)
        {
            var s = Section is {} ? await Section.GetAsync(propertyName) : null;
            if (!string.IsNullOrEmpty(s))
                return parser is {}
                    ? parser(s, out var sectionValue) ? sectionValue : useDefault! 
                    : await parseSectionValueAsync<T>(s, useDefault!, propertyName);

            return TryGetFieldValue<T>(propertyName, out var fieldValue, inherited) 
                ? fieldValue 
                : useDefault;
        }

        async Task<T?> parseSectionValueAsync<T>(string? stringValue, T useDefault, string propertyName)
        {
            foreach (var parser in s_valueParsers)
            {
                if (!parser(stringValue, typeof(T), out var parsedValue, useDefault))
                {
                    return (T?)Convert.ChangeType(parsedValue, typeof(T));
                }
            }

            return useDefault;
            
            // var s = stringValue?.Trim(); obsolete
            // if (typeof(T) == typeof(string))
            //     return string.IsNullOrEmpty(s)
            //         ? useDefault!
            //         : (T) Convert.ChangeType(s, typeof(T));
            //     
            // // automatically support IStringValue...
            // if (typeof(IStringValue).IsAssignableFrom(typeof(T)))
            //     return string.IsNullOrEmpty(s)
            //         ? useDefault
            //         : (T?) Convert.ChangeType(StringValueBase.MakeStringValue<T>(s), typeof(T))!; 
            //     
            // // automatically support boolean values 
            // if (typeof(T) == typeof(bool) && Section is {})
            //     return (await Section.GetAsync(propertyName)).TryParseConfiguredBool(out var boolValue)
            //         ? (T) Convert.ChangeType(boolValue, typeof(T))
            //         : useDefault!;
            //
            // // automatically support TimeSpan values 
            // if (typeof(T) == typeof(TimeSpan) && Section is {})
            //     return (await Section.GetAsync(propertyName)).TryParseConfiguredTimeSpan(out var timeSpanValue)
            //         ? (T) Convert.ChangeType(timeSpanValue, typeof(T))
            //         : useDefault!;
            //
            // var value = Section is {} ? await Section.GetAsync(propertyName) : null;
            
            
        }
        
        /// <summary>
        ///   Instantiates a <see cref="ConfigurationSection"/>.
        /// </summary>
        /// <param name="configuration">
        ///   The <see cref="IConfiguration"/> instance that contains the configuration section to be encapsulated.
        /// </param>
        /// <param name="log">
        ///   Initializes the <see cref="Log"/> value.
        /// </param>
        /// <param name="configPath">
        ///   (optional; default=value from <see cref="SectionIdentifier"/>)<br/>
        ///   Specifies the configuration section to be encapsulated. 
        /// </param>
        protected ConfigurationSection(
            IConfiguration? configuration, 
            ILog? log,
            ConfigPath? configPath = null)
        {
            ParentConfiguration = configuration;
            if (configPath?.StringValue?.Contains(configPath.Separator) ?? false)
            {
                SectionIdentifier = configPath.CopyLast()!;
                ConfigPath = configPath;
            }
            else
            {
                SectionIdentifier = configPath!;
                ConfigPath = ParentConfiguration is IConfigurationSection section
                    ? $"{section.Path}:{configPath}"
                    : configPath; 
                
            }

            var sectionKey = getSectionKey(configPath, configuration);
            Section = configuration?.GetSectionAsync(sectionKey).Result;
            IsEmpty = Section?.IsEmpty().Result ?? true; 
            Log = log;
            ConfigPath = configPath;
        }

        /// <summary>
        ///   Initializes the configuration section.
        /// </summary>
#pragma warning disable 8618
        protected ConfigurationSection()
        {
            addDefaultValueParsers();
        }

        void addDefaultValueParsers()
        {
            // automatically support ...
            s_valueParsers.AddRange(new ArbitraryValueParser[]
            {
                // string
                (string? stringValue, Type tgtType, out object o, object useDefault) =>
                {
                    if (Section is null || tgtType != typeof(string))
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim();
                    o = string.IsNullOrEmpty(s) ? useDefault : s;
                    return true;
                },
                
                // IStringValue
                (string? stringValue, Type tgtType, out object o, object useDefault) =>
                {
                    if (Section is null || !typeof(IStringValue).IsAssignableFrom(tgtType))
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim();
                    o = string.IsNullOrEmpty(s) ? useDefault : StringValueBase.MakeStringValue(tgtType, s);
                    return true;
                },

                // boolean
                (string? stringValue, Type tgtType, out object o, object useDefault) =>
                {
                    if (Section is null || tgtType != typeof(bool))
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim();
                    if (string.IsNullOrEmpty(s))
                    {
                        o = useDefault;
                        return true;
                    }

                    if (s!.TryParseConfiguredBool(out var boolValue))
                    {
                        o = boolValue;
                        return true;
                    }

                    o = null!;
                    return false;
                },
                (string? stringValue, Type tgtType, out object o, object useDefault) =>
                {
                    if (Section is null || tgtType != typeof(bool))
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim();
                    if (string.IsNullOrEmpty(s))
                    {
                        o = useDefault;
                        return true;
                    }

                    if (s!.TryParseConfiguredTimeSpan(out var timeSpanValue))
                    {
                        o = timeSpanValue;
                        return true;
                    }

                    o = null!;
                    return false;
                }
            });

        }
#pragma warning restore 8618
    }
}