using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TetraPak.XP.Configuration;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

[assembly:XpService(typeof(ConfigurationSection))]

namespace TetraPak.XP.Configuration
{
    /// <summary>
    ///   Provides access to the configuration framework through a POCO class. 
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(ConfigurationSectionJsonConverter))]
    [JsonConvertDynamicEntities(FactoryType = typeof(ConfigurationSectionFactory))]
    //[DebuggerDisplay("{" + nameof(Path) + "}")] nisse - restore
    public class ConfigurationSection : DynamicEntity, IConfigurationSection, IConfigurationSectionExtended
    {
        const string RootKey = ".";
        static readonly List<ArbitraryValueParser> s_valueParsers = getDefaultValueParsers();
        string? _path;
        ILog? _log;
        string _key = RootKey;

        /// <summary>
        /// Gets the key this section occupies in its parent.
        /// </summary>
        public string Key
        {
            get => _key;
            internal set
            {
                _key = value;
                invalidatePath();
            }
        }

        /// <summary>
        /// Gets the full path to this section within the <see cref="Configuration.IConfiguration"/>.
        /// </summary>
        public ConfigPath Path => _path ??= buildPath();

        ConfigPath buildPath() =>
            ParentConfiguration is null 
                ? Key != RootKey ? new ConfigPath(Key) : ConfigPath.Empty 
                : (ConfigPath) new ConfigPath(ParentConfiguration.Path).Push(Key);
        // ParentConfiguration is not IConfigurationSection parentSection obsolete
        //     ? Key is {} ? new ConfigPath(Key) : ConfigPath.Empty 
        // : (ConfigPath) new ConfigPath(parentSection.Path).Push(Key!);

        /// <summary>
        /// Gets or sets the section value.
        /// </summary>
        public string? Value 
        {
            get => Get<string?>();
            set => Set(value);
        }

        /// <summary>
        ///   Gets a value that indicates whether the configuration section contains no information. 
        /// </summary>
        [JsonIgnore]
        public bool IsEmpty => Count == 0;

        /// <summary>
        ///   Can be overridden. Returns the expected configuration section identifier like in this example:<br/>
        ///   <code>
        ///   "MySection": {
        ///     :
        ///   }
        ///   </code>
        /// </summary>
        public string? SectionIdentifier { get; protected set; }

        /// <summary>
        ///   Gets the parent <see cref="Configuration.IConfiguration"/> section
        ///   (or <c>null</c> if this section is also the configuration root).
        /// </summary>
        internal IConfigurationSection? ParentConfiguration { get; set; }

        /// <summary>
        ///   Gets a logger.
        /// </summary>
        public ILog? Log => _log ?? getParentLog();

        ILog? getParentLog() =>
            ParentConfiguration is ConfigurationSection parentSection
                ? parentSection.Log
                : null;
        
        string getSectionKey(ConfigPath? sectionIdentifier, IConfiguration? configuration)
        {
            if (sectionIdentifier?.IsEmpty ?? true)
            {
                sectionIdentifier = SectionIdentifier!;
            }
            if (sectionIdentifier.Count == 1)
                return sectionIdentifier;

            if (configuration is not IConfigurationSection section) 
                return sectionIdentifier;
            
            var sectionPath = section.Path;
            return sectionIdentifier.StringValue.StartsWith(sectionPath) 
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
        protected Task<T?> GetFromFieldThenSectionAsync<T>(
            T useDefault = default!, 
            TypedValueParser<T>? parser = null, 
            bool inherited = true,
            [CallerMemberName] string propertyName = null!)
        {
            return Task.FromResult(TryGetFieldValue<T>(propertyName, out var fieldValue, inherited) 
                ? fieldValue 
                : GetValue<T>(propertyName));
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
        protected Task<T?> GetFromSectionThenField<T>(
            T? useDefault = default, 
            TypedValueParser<T>? parser = null, 
            bool inherited = true,
            [CallerMemberName] string propertyName = null!)
        {
            var value = GetValue(propertyName, useDefault);
            if (value is { })
                return Task.FromResult(value)!;
            
            return Task.FromResult(TryGetFieldValue<T>(propertyName, out var fieldValue, inherited) 
                ? fieldValue 
                : useDefault);
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
        protected T? GetFromFieldThenSection<T>(
            T useDefault = default!, 
            ValueParser<T>? parser = null, 
            bool inherited = true,
            [CallerMemberName] string propertyName = null!)
        {
            if (TryGetFieldValue<T>(propertyName, out var fieldValue, inherited))
                return fieldValue;

            return GetValue<T?>(propertyName);
        }

        public virtual Task<T?> GetAsync<T>(string key, T? useDefault = default) => Task.FromResult(GetValue(key, useDefault));

        public override T? GetValue<T>(string key, T? useDefault = default) where T : default
        {
            var path = new ConfigPath(key);
            object? obj;
            if (path.Count != 1)
            {
                // obtain from child entity ...
                key = path.Root;
                obj = base.GetValue<object>(key);
                if (obj is null)                    
                    return useDefault;

                if (obj is ConfigurationSection section)
                    return section.GetValue(path.Pop(1, SequentialPosition.Start), useDefault);

                if (obj is DynamicEntity entity)
                    return getEntityValue(entity, path.Pop(1, SequentialPosition.Start), useDefault);
            }
            
            if (typeof(T) == typeof(string))
                return base.GetValue(key, useDefault);

            obj = base.GetValue<object>(key);
            switch (obj)
            {
                case T t:
                    return t;
                case null:
                    return useDefault;
            }

            foreach (var parser in s_valueParsers)
            {
                if (parser(key, typeof(T), out obj, null!) && obj is T tValue)
                    return tValue;
            }

            return useDefault;
        }

        T? getEntityValue<T>(DynamicEntity entity, DynamicPath path, T? useDefault)
        {
            if (path.Count == 1)
                return entity.GetValue(path.StringValue, useDefault);

            var child = entity.GetValue<object>(path.Root);
            if (child is DynamicEntity childEntity)
                return childEntity.GetValue(path.Pop(1, SequentialPosition.Start), useDefault);

            return useDefault;
        }

        // public override void SetValue<TValue>(string key, TValue? value) where TValue : default
        // {
        //     base.SetValue(key, value);
        //
        //     // // attach or detach a child section ... obsolete
        //     // if (value is ConfigurationSection child)
        //     // {
        //     //     child.AttachToParent(this, key);
        //     //     return;
        //     // }
        //     //
        //     // if (value is null && TryGetValue(key, out var obj) && obj is ConfigurationSection section)
        //     // {
        //     //     section.detachFromParent();
        //     //     return;
        //     // }
        //
        //     // if value is 'Key' we need to invalidate the path ...
        //     if (key == nameof(key))
        //     {
        //         invalidatePath();
        //     }
        // }

        void detachFromParent()
        {
            ParentConfiguration = null;
            _path = null;
        }

        void invalidatePath() => _path = null;
        
#if DEBUG
        public void InvalidatePath() // nisse remove when buildPath() works as intended
        {
            invalidatePath();
        }
#endif
        
        public Task<string> GetAsync(string key, string? useDefault = null) => Task.FromResult(GetValue(key, useDefault)!);

        public Task SetAsync(string key, string value)
        {
            SetValue(key, value);
            return Task.CompletedTask;
        }

        public Task<IConfigurationSection> GetSectionAsync(string key) => Task.FromResult(GetValue<IConfigurationSection>(key))!;

        public Task<IEnumerable<IConfigurationSection>> GetChildrenAsync()
        {
            var children = new List<IConfigurationSection>();
            foreach (var pair in this)
            {
                if (pair.Value is IConfigurationSection child)
                {
                    children.Add(child);
                }
            }

            return Task.FromResult<IEnumerable<IConfigurationSection>>(children);
        }
        
        /// <summary>
        ///   Initializes the configuration section.
        /// </summary>
        public ConfigurationSection(ILog? log = null)
        {
            _log = log;
        }

        static List<ArbitraryValueParser> getDefaultValueParsers()
        {
            // automatically support ...
            return new ArbitraryValueParser[]
            {
                // string
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (tgtType != typeof(string))
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim();
                    o = string.IsNullOrEmpty(s) ? useDefault : s;
                    return true;
                },
                
                // IStringValue
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (!typeof(IStringValue).IsAssignableFrom(tgtType))
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim();
                    o = string.IsNullOrEmpty(s) ? useDefault : StringValueBase.MakeStringValue(tgtType, s);
                    return true;
                },

                // boolean
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (tgtType != typeof(bool))
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
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (tgtType != typeof(TimeSpan))
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
            }.ToList();

        }
#pragma warning restore 8618
    }
}