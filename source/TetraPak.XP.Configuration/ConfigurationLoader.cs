﻿using System;
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
        
        public Task<IConfigurationSection?> LoadFromAsync(
            DirectoryInfo? folder = null,
            ILog? log = null,
            RuntimeEnvironment? environment = null)
        {
            folder ??= new DirectoryInfo(Environment.CurrentDirectory);
            environment ??= _runtimeEnvironmentResolver.ResolveRuntimeEnvironment();
            var filename = environment == RuntimeEnvironment.Production
                ? "appsettings.json"
                : $"appsettings.{environment.ToString()}.json";
            return LoadFromAsync(new FileInfo(Path.Combine(folder.FullName, filename)));
        }

        public Task<IConfigurationSection?> LoadFromAsync(
            string path,
            ILog? log = null,
            RuntimeEnvironment? environment = null)
        {
            if (File.Exists(path))
                return LoadFromAsync(new FileInfo(path));

            if (Directory.Exists(path))
                return LoadFromAsync(new DirectoryInfo(path), log, environment);

            throw new DirectoryNotFoundException($"Path not found: {path}");
        }

        public static async Task<IConfigurationSection?> LoadFromAsync(FileInfo file)
        {
            if (!file.Exists)
                throw new FileNotFoundException($"Could not find configuration file: {file.FullName}");

            using var stream = file.OpenRead();
            try
            {
                var configSection = await JsonSerializer.DeserializeAsync<ConfigurationSection>(stream); 
                return buildGraph(configSection!);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
        }
    }
}