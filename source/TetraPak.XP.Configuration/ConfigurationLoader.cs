using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Configuration
{
    // todo Support overloading configuration from specialized 'Environment' files (eg. appsettings.Development.json)
    // todo Support overloading configuration from environment variables (eg. "TetraPak:GrantType") 
    public class ConfigurationLoader  
    {
        readonly IRuntimeEnvironmentResolver _runtimeEnvironmentResolver;
        readonly ILog? _log;

        public async Task<IConfigurationSection> LoadFromAsync(
            DirectoryInfo? folder = null,
            ILog? log = null,
            RuntimeEnvironment? environment = null)
        {
            folder ??= new DirectoryInfo(Environment.CurrentDirectory);
            var file = new FileInfo(Path.Combine(folder.FullName, "appsettings.json"));
            log.Trace($"Loads configuration from {file.FullName}");
            var config = await LoadFromAsync(file, log);
            environment ??= _runtimeEnvironmentResolver.ResolveRuntimeEnvironment();
            return environment == RuntimeEnvironment.Production
                ? config
                : await overloadFromAsync(config, folder, log, environment);
        }

        async Task<IConfigurationSection> overloadFromAsync(
            IConfigurationSection configuration,
            DirectoryInfo? folder = null,
            ILog? log = null,
            RuntimeEnvironment? environment = null)
        {
            folder ??= new DirectoryInfo(Environment.CurrentDirectory);
            environment ??= _runtimeEnvironmentResolver.ResolveRuntimeEnvironment();
            var filename = environment == RuntimeEnvironment.Production
                ? "appsettings.json"
                : $"appsettings.{environment.ToString()}.json";
            var file = new FileInfo(Path.Combine(folder.FullName, filename));
            if (!file.Exists)
                return configuration;
            
            var overloadConfig = await LoadFromAsync(file, log);
            return await configuration.OverloadAsync(overloadConfig);
        }

        public Task<IConfigurationSection> LoadFromAsync(
            string path,
            ILog? log = null,
            RuntimeEnvironment? environment = null)
        {
            if (File.Exists(path))
                return LoadFromAsync(new FileInfo(path), log);

            if (Directory.Exists(path))
                return LoadFromAsync(new DirectoryInfo(path), log, environment);

            throw new DirectoryNotFoundException($"Path not found: {path}");
        }

        public static async Task<IConfigurationSection> LoadFromAsync(FileInfo file, ILog? log)
        {
            if (!file.Exists)
                throw new FileNotFoundException($"Could not find configuration file: {file.FullName}");

#if NET5_0_OR_GREATER            
            await using var stream = file.OpenRead();
#else
            using var stream = file.OpenRead();
#endif            
            try
            {
                var configSection = await JsonSerializer.DeserializeAsync<ConfigurationSection>(stream); 
                return buildGraph(configSection!);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        static IConfigurationSection buildGraph(ConfigurationSection rootSection)
        {
            foreach (var pair in rootSection)
            {
                if (pair.Value is IConfigurationSection childSection)
                {
                    attachToParent(childSection, rootSection);
                }
            }
            
            void attachToParent(IConfigurationSection section, IConfigurationSection parent)
            {
                if (section is not ConfigurationSection subSection)
                    return;

                subSection.ParentConfiguration = parent;
                subSection.BuildPath();
                foreach (var pair in subSection)
                {
                    if (pair.Value is not ConfigurationSection childSection)
                        continue;
                    
                    attachToParent(childSection, section);
                }
            }

            return rootSection;
        }

        public ConfigurationLoader(IRuntimeEnvironmentResolver runtimeEnvironmentResolver, ILog? log = null)
        {
            _runtimeEnvironmentResolver = runtimeEnvironmentResolver;
            _log = log;
        }
    }
}