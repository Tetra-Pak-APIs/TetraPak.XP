using System;
using System.Linq;
using System.Threading.Tasks;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Configuration
{
    /// <summary>
    ///   This configuration value delegate can be inserted into the configuration framework
    ///   to support referencing values from different sources, such as environment variables, in configuration files.
    /// </summary>
    /// <seealso cref="Configure.InsertValueDelegate"/>
    public class ConfigurationValueSourceDelegate : IConfigurationValueDelegate
    {
        const string EnvironmentIdent = "Environment";

        public bool IsFallbackDelegate => false;

        /// <inheritdoc />
        public virtual Outcome<T> GetValue<T>(ConfigurationValueArgs<T> args)
        {
            var value = args.Configuration[args.Key]?.Trim();
            if (value.IsUnassigned())
                return Outcome<T>.Fail(new Exception($"No '{args.Key}' value found in configuration"));

            if (!value!.StartsWith("$(") || !value.EndsWith(')')) 
                return Outcome<T>.Success((T) (object) value);
    
            var ms = new DynamicPath(value.Substring(2).TrimEnd(')'), "/");
            if (ms.Count == 1)
                return getValueFromEnvironmentVariables<T>(ms);

            var source = ms.Root; 
            return source switch
            {
                EnvironmentIdent => getValueFromEnvironmentVariables<T>(ms.Pop(1, SequentialPosition.Start)),
                _ => Outcome<T>.Fail(new NotSupportedException($"Variable source is not supported for {value}"))
            };
        }

        // todo consider supporting more variable sources 
        static Outcome<T> getValueFromEnvironmentVariables<T>(IStringValue path)
        {
            var key = path.StringValue;
            var value = Environment.GetEnvironmentVariable(key)?.Trim();
            return value.IsUnassigned()
                ? Outcome<T>.Fail($"Environment variable not found: {path}")
                : Outcome<T>.Success((T) (object) value!);
        }
    }

    /// <summary>
    ///   This configuration value delegate can be inserted into the configuration framework
    ///   to support different values depending on the current runtime platform, allowing for different configurations
    ///   for Windows, MacOS, Linux, iOS, Android etc. 
    /// </summary>
    /// <seealso cref="Configure.InsertValueDelegate"/>
    public class ConfigurationVariablePlatformValueDelegate : IConfigurationValueDelegate
    {
        RuntimePlatform _runtimePlatform;
        const string Qualifier = "$OnPlatform";
        const string Prefix = "{";
        const string Suffix = "}";
        
        public bool IsFallbackDelegate => false;
        public Outcome<T> GetValue<T>(ConfigurationValueArgs<T> args)
        {
            // syntax: '$OnPlatform' '{' <PlatformAndValuesList> '}'
            //         <PlatformAndValuesList> := <Platform>'='<Value>
            //         <Platform> := enum (TetraPak.XP.RuntimePlatform)
            //         <Value> := string

            var value = args.Configuration[args.Key]?.Trim();
            if (value.IsUnassigned())
                return Outcome<T>.Fail(new Exception($"No '{args.Key}' value found in configuration"));

            if (!value!.StartsWith(Qualifier)) 
                return Outcome<T>.Success((T) (object) value);

            if (!value.TryEatEnclosed(Prefix, Suffix, out var values, Qualifier.Length))
                return Outcome<T>.Success((T) (object) value);
            
            var platformAndValues = values!.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            if (!platformAndValues.Any())
                return Outcome<T>.Fail("No platform values specified");

            foreach (var platformAndValue in platformAndValues)
            {
                var pair = platformAndValue.Trim();
                var split = pair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length != 2)
                    continue;

                var platform = split[0];
                if (platform.TryParseEnum<RuntimePlatform>(out var runtimePlatform, true) 
                        && runtimePlatform == _runtimePlatform
                        && args.TryParse(split[1], out var typedValue))
                    return Outcome<T>.Success(typedValue);
            }
            
            return Outcome<T>.Success((T) (object) value);
        }

        void resolveRuntimePlatform()
        {
            _runtimePlatform = XpServices.GetRequired<IPlatformService>().RuntimePlatform;
        }

        public ConfigurationVariablePlatformValueDelegate()
        {
#pragma warning disable CS4014
            resolveRuntimePlatform();
#pragma warning restore CS4014
        }

    }
}