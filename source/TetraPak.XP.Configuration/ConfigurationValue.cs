using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TetraPak.XP.Configuration
{
    public class ConfigurationValue : IConfigurationValue
    {
        /// <inheritdoc />
        public string Key { get; }
        
        /// <inheritdoc />
        public string Path { get; }
        
        /// <inheritdoc />
        public object? Value { get; set; }

        public Task<TValue?> GetAsync<TValue>(string key, TValue? useDefault = default)
        {
            throw new InvalidOperationException($"Configuration value doesn't have sub values");
        }

        public Task SetAsync(string key, object? value)
        {
            throw new InvalidOperationException($"Configuration value doesn't have sub values");
        }

        public Task<IConfigurationSection?> GetSectionAsync(string key)
        {
            throw new InvalidOperationException($"Configuration value doesn't have sub values");
        }

        public Task<IEnumerable<IConfigurationItem>> GetChildrenAsync()
        {
            throw new InvalidOperationException($"Configuration value doesn't have sub values");
        }

        public ConfigurationValue(IConfigurationItem parent, string key, object? value)
        {
            Key = key;
            Path = new ConfigPath(parent.Path).Push(Key).StringValue;
            Value = value;
        }
    }
}