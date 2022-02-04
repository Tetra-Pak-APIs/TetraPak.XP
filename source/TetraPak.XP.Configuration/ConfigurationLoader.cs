using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Configuration
{
    public static class ConfigurationLoader
    {
        const string TetraPakAppEnvironmentVariable = "TETRAPAK_ENVIRONMENT";
        
        public static Task<IConfigurationSection?> LoadFromAsync(DirectoryInfo? folder = null,
            ILog? log = null,
            RuntimeEnvironment? environment = null)
        {
            folder ??= new DirectoryInfo(Environment.CurrentDirectory);
            environment ??= resolveRuntimeEnvironment(RuntimeEnvironment.Production);
            var filename = environment == RuntimeEnvironment.Production
                ? "appsettings.json"
                : $"appsettings.{environment.ToString()}.json";
            return LoadFromAsync(new FileInfo(Path.Combine(folder.FullName, filename)), log);
        }

        public static Task<IConfigurationSection?> LoadFromAsync(
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

        public static async Task<IConfigurationSection?> LoadFromAsync(FileInfo file, ILog? log)
        {
            if (!file.Exists)
                throw new FileNotFoundException($"Could not find configuration file: {file.FullName}");

            using var stream = file.OpenRead();
            try
            {
                return buildGraph(await JsonSerializer.DeserializeAsync<ConfigurationSection>(stream));
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
                    attachToParent(childSection, rootSection/*, pair.Key*/);
                }
            }
            
            void attachToParent(IConfigurationSection section, IConfigurationSection parent/*, string key*/)
            {
                if (section is not ConfigurationSection subSection)
                    return;

                subSection.ParentConfiguration = parent;
                // subSection.Key = key;
                foreach (var pair in subSection)
                {
                    if (pair.Value is not ConfigurationSection childSection)
                        continue;
                    
                    attachToParent(childSection, section/*, pair.Key*/);
                    var nisse = childSection.Path;
                }
            }

            return rootSection;
        }

        static RuntimeEnvironment resolveRuntimeEnvironment(RuntimeEnvironment useDefault)
        {
            var s = Environment.GetEnvironmentVariable(TetraPakAppEnvironmentVariable);
            if (!s.IsAssigned())
                return useDefault;

            return Enum.TryParse<RuntimeEnvironment>(s, true, out var value)
                ? value
                : useDefault;
        }
    }
}