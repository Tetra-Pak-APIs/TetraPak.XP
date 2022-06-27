using System;
using Microsoft.Extensions.Configuration;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Configuration
{
    public sealed class ConfigurationSectionDecoratorArgs
    {
        public ILog? Log { get; }

        public IConfiguration Configuration { get; }

        public IRuntimeEnvironmentResolver RuntimeEnvironmentResolver { get; }

        public ConfigurationSectionDecorator? Parent { get; }

        public IConfigurationSection Section { get; }
        
        public static ConfigurationSectionDecoratorArgs ForSubSection(string key)
            => ForSubSection(null, key);

        public static ConfigurationSectionDecoratorArgs ForSubSection(
            ConfigurationSectionDecorator? parent,
            string key)
        {
            var section = parent is { }
                ? parent.GetSubSection(key)
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
            ConfigurationSectionDecorator? parent,
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