using System;
using TetraPak.XP.Configuration;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   This configuration value delegate can be inserted into the configuration framework
    ///   to support referencing environment variables in configuration files.
    /// </summary>
    /// <seealso cref="Configure.InsertValueDelegate"/>
    public sealed class CredentialsConfigurationVariableValueDelegate : ConfigurationVariableValueDelegate
    {
        /// <inheritdoc />
        public override Outcome<T> GetValue<T>(ConfigurationValueArgs<T> args)
        {
            if (!isClientCredentials(args) || typeof(T) != typeof(string))
                return Outcome<T>.Fail(new Exception("Not concerned with this"));

            return base.GetValue(args);
        }

        static bool isClientCredentials<T>(ConfigurationValueArgs<T> args)
        {
            return args.Key.EndsWith(nameof(IAuthConfiguration.ClientId)) || args.Key.EndsWith(nameof(IAuthConfiguration.ClientSecret));
        }
    }
}