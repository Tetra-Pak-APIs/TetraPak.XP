using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TetraPak.XP.Logging;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace TetraPak.XP.Configuration
{
    public class ConfigurationSectionWrapper : IConfigurationSectionExtended
    {
        protected readonly IConfigurationSectionExtended? Section;

        public string Key => Section?.Key ?? string.Empty;

        public string Path => Section?.Path ?? string.Empty;
        
        public ILog? Log { get; }

        public int Count => Section?.Count ?? 0;

        public TValue? Get<TValue>(TValue? useDefault = default, [CallerMemberName] string? caller = null)
        {
            return Section is { } ? Section.Get(useDefault, caller) : useDefault;
        }

        public TValue? GetValue<TValue>(string key, TValue? useDefault = default)
        {
            return Section is { } ? Section.GetValue(key, useDefault) : useDefault;
        }

        public async Task<TValue?> GetAsync<TValue>(string key, TValue? useDefault = default)
        {
            return Section is { } ? await Section.GetAsync(key, useDefault) : useDefault;
        }

        public Task SetAsync(string key, object? value)
        {
            return Section is null 
                ? Task.CompletedTask 
                : Section.SetAsync(key, value);
        }

        public Task<IConfigurationSection?> GetSectionAsync(string key)
        {
            return Section is {}
                ? Section.GetSectionAsync(key) 
                : Task.FromResult<IConfigurationSection?>(null);
        }

        public Task<IEnumerable<IConfigurationItem>> GetChildrenAsync()
        {
            return Section is {} 
                ? Section.GetChildrenAsync() 
                : Task.FromResult<IEnumerable<IConfigurationItem>>(Array.Empty<IConfigurationSection>());
        }

        protected virtual Task<T?> GetFromFieldThenSectionAsync<T>(
            T useDefault = default!,
            TypedValueParser<T>? parser = null,
            bool inherited = true,
            [CallerMemberName] string propertyName = null!)
        {
            if (Section is not ConfigurationSection cs)
                throw new InvalidOperationException($"Wrapped configuration section must derive from {typeof(ConfigurationSection)}");
            
            return cs.GetFromFieldThenSectionAsync(useDefault, parser, inherited, propertyName);
        }
        
#if NET5_0_OR_GREATER
        protected bool TryGetFieldValue<T>(string propertyName, [NotNullWhen(true)] out T value, bool inherited = false)
#else
        protected bool TryGetFieldValue<T>(string propertyName, out T value, bool inherited = false)
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

            if (fieldInfo is { })
                return fieldInfo;

            if (Section is ConfigurationSection cs)
                return cs.GetField(fieldName, inherited);

            return null;
        }

        public ConfigurationSectionWrapper(IConfigurationSectionExtended section, ILog? log)
        {
            Section = section;
            Log = log;
        }
        
        public static string ValidateAssigned(string key)
        {
            if (key.IsUnassigned()) 
                throw new ArgumentException($"Configuration key was unassigned");

            return key;
        }


        public ConfigurationSectionWrapper(IConfiguration? configuration, string? key, ILog? log = null)
        {
            Log = log;
            if (configuration is null)
            {
                Section = null;
            }

            if (configuration is { } && key.IsAssigned())
            {
                Section = configuration.GetSectionAsync(key!).Result as IConfigurationSectionExtended 
                          ?? throw new ArgumentOutOfRangeException(nameof(key), 
                              $"Cannot resolve configuration section from '{key}'");
            }
        }
    }
}