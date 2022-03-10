using System;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.DynamicEntities;

namespace authClient.console
{
    class ConfigurationVariablesDelegate : IConfigurationValueDelegate
    {
        const string EnvironmentIdent = "Env";

        public bool IsFallbackDelegate => false;

        public Outcome<T> GetValue<T>(ConfigurationValueArgs<T> args)
        {
            if (!isClientCredentials(args) || typeof(T) != typeof(string))
                return Outcome<T>.Fail(new Exception("Not concerned with this"));

            var value = args.Configuration[args.Key]?.Trim();
            if (string.IsNullOrEmpty(value))
                return Outcome<T>.Fail(new Exception($"No '{args.Key}' value found in configuration"));

            if (!value.StartsWith("$(") || !value.EndsWith(')')) 
                return Outcome<T>.Success((T) (object) value);
    
            var ms = new DynamicPath(value[2..].TrimEnd(')'), "/");
            if (ms.Count == 1)
                return getValueFromEnvironmentVariables<T>(ms);

            var source = ms.Root; 
            return source switch
            {
                EnvironmentIdent => getValueFromEnvironmentVariables<T>(ms.Pop(1, SequentialPosition.Start)),
                _ => Outcome<T>.Fail(new NotSupportedException($"Variable source is not supported for {value}"))
            };

        }

        static bool isClientCredentials<T>(ConfigurationValueArgs<T> args)
        {
            return args.Key.EndsWith(nameof(IAuthConfiguration.ClientId)) || args.Key.EndsWith(nameof(IAuthConfiguration.ClientSecret));
        }

        // todo consider supporting more variable sources 
        static Outcome<T> getValueFromEnvironmentVariables<T>(IStringValue path)
        {
            var key = path.StringValue;
            var value = Environment.GetEnvironmentVariable(key)?.Trim();
            return string.IsNullOrEmpty(value)
                ? Outcome<T>.Fail($"Environment variable not found: {path}")
                : Outcome<T>.Success((T) (object) value);
        }
    }
}