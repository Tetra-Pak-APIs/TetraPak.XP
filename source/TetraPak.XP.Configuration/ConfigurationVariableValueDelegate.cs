using System;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Configuration
{
    /// <summary>
    ///   This configuration value delegate can be inserted into the configuration framework
    ///   to support referencing environment variables in configuration files.
    /// </summary>
    /// <seealso cref="Configure.InsertValueDelegate"/>
    public class ConfigurationVariableValueDelegate : IConfigurationValueDelegate
    {
        const string EnvironmentIdent = "Environment";

        public bool IsFallbackDelegate => false;

        /// <inheritdoc />
        public virtual Outcome<T> GetValue<T>(ConfigurationValueArgs<T> args)
        {
            // if (!isClientCredentials(args) || typeof(T) != typeof(string)) obsolete
            //     return Outcome<T>.Fail(new Exception("Not concerned with this"));

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

        // static bool isClientCredentials<T>(ConfigurationValueArgs<T> args) obsolete
        // {
        //     return args.Key.EndsWith(nameof(IAuthConfiguration.ClientId)) || args.Key.EndsWith(nameof(IAuthConfiguration.ClientSecret));
        // }

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
}