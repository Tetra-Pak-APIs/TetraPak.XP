using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;
using Xunit;
using IConfigurationSection = TetraPak.XP.Configuration.IConfigurationSection;

namespace TetraPak.XP.Desktop.Tests
{
    public class DesktopConfigurationSectionTests
    {
        [Fact]
        public async Task Load_config_from_specified_file_and_assert_paths()
        {
            const string SettingsFile = "../../../_files/appsettings.basic.json";
            var config = await DesktopConfigurationLoader.LoadFromAsync(SettingsFile); 
            Assert.NotNull(config);
            // var collection = XpServices.NewServiceCollection();
            // collection.AddSingleton<IConfiguration>(_ => configRoot!);

            var children = (await config!.GetChildrenAsync()).ToArray();
            Assert.Single(children);
            var child = children[0];
            Assert.Equal("Logging", child.Path);
            children = (await child.GetChildrenAsync()).ToArray();
            Assert.Single(children);
            child = children[0];
            Assert.Equal("Logging:LogLevel", child.Path);

            var stringValue = await config.GetAsync<string>("TestString");
            Assert.NotNull(stringValue);
            Assert.Equal("Hello World!", stringValue);

            var stringArray = await config.GetAsync<string[]>("TestStringArray");
            Assert.NotNull(stringArray);
            Assert.Equal(3, stringArray!.Length);
            
            var numArray = await config.GetAsync<double[]>("TestIntArray");
            Assert.NotNull(numArray);
            Assert.Equal(2, numArray!.Length);

            var logLevel = await config.GetAsync<IExtendedConfigurationSection>("Logging:LogLevel");
            Assert.NotNull(logLevel);
            Assert.Equal(3, logLevel!.Count);
        }
    }

    static class DesktopConfigurationLoader
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

            await using var stream = file.OpenRead();
            try
            {
                return await JsonSerializer.DeserializeAsync<DesktopConfigurationSection>(stream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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