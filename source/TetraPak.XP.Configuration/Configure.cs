using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Configuration
{
    public static class Configure
    {
        static readonly List<ConfigurationSectionWrapperDelegate> s_wrapperDelegates = new();
        static readonly List<IConfigurationValueDelegate> s_valueDelegates = new();
        static readonly List<ValueParser> s_valueParsers = getDefaultValueParsers();

        internal static ConfigurationSectionWrapperDelegate[] GetSectionWrapperDelegates()
        {
            lock (s_wrapperDelegates)
                return s_wrapperDelegates.ToArray();
        }

        internal static IConfigurationValueDelegate[] GetValueDelegates()
        {
            lock (s_valueDelegates)
                return s_valueDelegates.ToArray();
        }
        
        internal static ValueParser[] GetValueParsers()
        {
            lock (s_valueParsers)
                return s_valueParsers.ToArray();
        }

        public static void InsertWrapperDelegate(ConfigurationSectionWrapperDelegate wrapperDelegate, int index = 0)
        {
            lock (s_wrapperDelegates)
            {
                if (s_wrapperDelegates.Contains(wrapperDelegate))
                    throw new ArgumentException("Delegate was already inserted", nameof(wrapperDelegate));

                s_wrapperDelegates.Insert(index, wrapperDelegate);
            }
        }

        public static void InsertValueDelegate(IConfigurationValueDelegate valueDelegate, int index = 0)
        {
            lock (s_valueDelegates)
            {
                if (s_valueDelegates.Contains(valueDelegate))
                   throw new ArgumentException("Delegate was already inserted", nameof(valueDelegate));

                s_valueDelegates.Insert(index, valueDelegate);
            }
        }
        
        static List<ValueParser> getDefaultValueParsers()
        {
            // automatically support ...
            return new ValueParser[]
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

#if NET5_0_OR_GREATER
                    if (s.TryParseConfiguredBool(out var boolValue))
#else                        
                    if (s!.TryParseConfiguredBool(out var boolValue))
#endif                        
                    {
                        o = boolValue;
                        return true;
                    }

                    o = null!;
                    return false;
                },
                
                // enum
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (tgtType.IsEnum)
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim(); // todo Consider supporting names with whitespace (make identifier). Eg: "Client Credentials" => "ClientCredentials"
                    if (string.IsNullOrEmpty(s))
                    {
                        o = useDefault;
                        return true;
                    }

#if NET5_0_OR_GREATER
                    if (s.TryParseEnum(tgtType, out o))
#else
                    if (s!.TryParseEnum(tgtType, out o))
#endif
                        return true;
                    
                    o = null;
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

#if NET5_0_OR_GREATER
                    if (s.TryParseConfiguredTimeSpan(out var timeSpanValue))
#else
                    if (s!.TryParseConfiguredTimeSpan(out var timeSpanValue))
#endif
                    {
                        o = timeSpanValue;
                        return true;
                    }

                    o = null!;
                    return false;
                }
            }.ToList();
        }
    }

    public delegate ConfigurationSectionWrapper? ConfigurationSectionWrapperDelegate(ConfigurationSectionWrapperArgs args);

    public interface IConfigurationValueDelegate
    {
        Outcome<T> GetValue<T>(ConfigurationValueArgs<T> args);
    }

    public class ConfigurationValueArgs<T>
    {
        readonly ValueParser[] _parsers;

        public IConfiguration Configuration { get; }

        public string Key { get; }

        public T DefaultValue { get; }


        public ILog? Log { get; }

        public bool Parse(string stringValue, out T value)
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
            T defaultValue, 
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

    public class ConfigurationSectionWrapperArgs
    {
        public ILog? Log { get; }

        public IConfiguration Configuration { get; }

        public IRuntimeEnvironmentResolver RuntimeEnvironmentResolver { get; }
            
        public ConfigurationSectionWrapper? Parent { get; }  
        
        public IConfigurationSection Section { get; }

        // public ConfigurationSectionWrapperArgs ForSubSection(ConfigurationSectionWrapper? parent, IConfigurationSection subSection)
        // {
        //     return new ConfigurationSectionWrapperArgs(
        //         parent, 
        //         Configuration, 
        //         subSection,
        //         RuntimeEnvironmentResolver,
        //         Log);
        // }

        public static ConfigurationSectionWrapperArgs CreateFromServices(ConfigurationSectionWrapper? parent, string key)
        {
            var conf = XpServices.GetRequired<IConfiguration>();
            return new ConfigurationSectionWrapperArgs(
                parent,
                XpServices.GetRequired<IConfiguration>(),
                conf.GetRequiredSubSection(key),
                XpServices.GetRequired<IRuntimeEnvironmentResolver>(),
                XpServices.Get<ILog>());
        }

        public ConfigurationSectionWrapperArgs(
            ConfigurationSectionWrapper? parent,
            IConfiguration conf,
            IConfigurationSection section,
            IRuntimeEnvironmentResolver runtimeEnvironmentResolver,
            ILog? log)
        {
            Parent = parent!;
            Configuration = conf ?? throw new ArgumentNullException(nameof(conf));
            Section = section ?? throw new ArgumentNullException(nameof(section));
            if (!Section.IsConfigurationSection())
                throw new InvalidOperationException("Cannot wrap configuration section of a value");

            RuntimeEnvironmentResolver = runtimeEnvironmentResolver;
            Log = log;
        }
    }

}