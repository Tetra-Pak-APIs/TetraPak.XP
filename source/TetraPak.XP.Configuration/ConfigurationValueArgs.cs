using Microsoft.Extensions.Configuration;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Configuration
{
    public sealed class ConfigurationValueArgs<T>
    {
        readonly ValueParser[] _parsers;

        public IConfiguration Configuration { get; }

        public string Key { get; }

        public T? DefaultValue { get; }

        public ILog? Log { get; }

        public bool TryParse(string stringValue, out T? value)
        {
            foreach (var parser in _parsers)
            {
                if (!parser(stringValue, typeof(T), out var obj, DefaultValue!) || obj is not T tValue)
                    continue;

                value = tValue;
                return true;
            }

            value = DefaultValue;
            return false;
        }

        internal ConfigurationValueArgs(
            IConfiguration configuration,
            string key,
            T? defaultValue,
            ValueParser[] parsers,
            ILog? log)
        {
            Configuration = configuration;
            Key = key;
            DefaultValue = defaultValue;
            _parsers = parsers;
            Log = log;
        }
    }
}