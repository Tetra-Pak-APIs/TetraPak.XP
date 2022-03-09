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
        static readonly List<IConfigurationDecoratorDelegate> s_decorators = new();
        static readonly List<IConfigurationValueDelegate> s_valueDelegates = new();
        static readonly List<ValueParser> s_valueParsers = getDefaultValueParsers();

        internal static IConfigurationDecoratorDelegate[] GetConfigurationDecorators()
        {
            lock (s_decorators)
                return s_decorators.ToArray();
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

        public static void InsertConfigurationDecorator(IConfigurationDecoratorDelegate decoratorDelegate, int index = -1)
        {
            lock (s_decorators)
            {
                if (s_decorators.Contains(decoratorDelegate))
                    throw new ArgumentException("Decorator was already inserted", nameof(decoratorDelegate));

                if (index >= 0)
                {
                    s_decorators.Insert(index, decoratorDelegate);
                    return;
                }

                if (!decoratorDelegate.IsFallbackDecorator || s_decorators.Count == 0)
                {
                    s_decorators.Insert(0, decoratorDelegate);
                    return;
                }

                // insert fallback decorator ...
                var lastIndex = s_decorators.Count - 1;
                for (var i = s_decorators.Count-1; i >= 0; i--)
                {
                    var decorator = s_decorators[i];
                    if (decorator.IsFallbackDecorator)
                    {
                        s_decorators.Insert(lastIndex, decoratorDelegate);
                        return;
                    }

                    lastIndex = i;
                }
                s_decorators.Insert(0, decoratorDelegate);
            }
        }

        public static void InsertValueDelegate(IConfigurationValueDelegate valueDelegate, int index = -1)
        {
            lock (s_valueDelegates)
            {
                if (s_valueDelegates.Contains(valueDelegate))
                   throw new ArgumentException("Delegate was already inserted", nameof(valueDelegate));

                if (index >= 0)
                {
                    s_valueDelegates.Insert(index, valueDelegate);
                    return;
                }
                
                if (!valueDelegate.IsFallbackDelegate || s_valueDelegates.Count == 0)
                {
                    s_valueDelegates.Insert(0, valueDelegate);
                    return;
                }
                
                // insert fallback delegate ...
                for (var i = s_valueDelegates.Count-1; i >= 0; i--)
                {
                    var del = s_valueDelegates[i];
                    if (del.IsFallbackDelegate) 
                        continue;
                    
                    if (i + 1 > s_valueDelegates.Count - 1)
                    {
                        s_valueDelegates.Add(valueDelegate);
                        return;
                    }
                    s_valueDelegates.Insert(i+1, valueDelegate);
                    return;
                }
                s_valueDelegates.Insert(0, valueDelegate);
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

    public interface IConfigurationDecoratorDelegate
    {
        bool IsFallbackDecorator { get; }

        Outcome<ConfigurationSectionWrapper> WrapSection(ConfigurationSectionDecoratorArgs args);
    }
    
    // public delegate ConfigurationSectionWrapper? ConfigurationSectionWrapperDelegate(ConfigurationSectionDecoratorArgs args); obsolete

    public interface IConfigurationValueDelegate
    {
        bool IsFallbackDelegate { get; }
        
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

    public class ConfigurationSectionDecoratorArgs
    {
        public ILog? Log { get; }

        public IConfiguration Configuration { get; }

        public IRuntimeEnvironmentResolver RuntimeEnvironmentResolver { get; }
            
        public ConfigurationSectionWrapper? Parent { get; }  
        
        public IConfigurationSection Section { get; }

        public static ConfigurationSectionDecoratorArgs ForSubSection(
            ConfigurationSectionWrapper? parent, 
            string key)
        {
            var path = new ConfigPath(key);
            if (path.Count != 1) 
                throw new ArgumentException($"Unexpected sub section key: '{key}'", nameof(key));
            
            var section = parent is { }
                ? parent.GetSection()
                : XpServices.GetRequired<IConfiguration>().GetSubSection(key);
            if (section is { })
                return new ConfigurationSectionDecoratorArgs(
                    parent,
                    XpServices.GetRequired<IConfiguration>(),
                    section,
                    XpServices.GetRequired<IRuntimeEnvironmentResolver>(),
                    XpServices.Get<ILog>());

            throw new ArgumentException($"Sub section not found: '{key}'", nameof(key));
        }

        public ConfigurationSectionDecoratorArgs(
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