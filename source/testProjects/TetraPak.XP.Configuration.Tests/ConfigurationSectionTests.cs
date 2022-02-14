using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TetraPak.XP.Configuration;
using TetraPak.XP.DependencyInjection;
using Xunit;

namespace TetraPak.XP.Desktop.Tests
{
    public class ConfigurationSectionTests
    {
        [Fact]
        public async Task Load_config_from_specified_file_and_assert_paths()
        {
            const string SettingsFile = "../../../_files/appsettings.json";
            var environmentResolver = XpServices.GetRequired<IRuntimeEnvironmentResolver>();
            var loader = new ConfigurationLoader(environmentResolver);
            var config = await loader.LoadFromAsync(SettingsFile);
            Assert.NotNull(config);
            // var collection = XpServices.NewServiceCollection();
            // collection.AddSingleton<IConfiguration>(_ => configRoot!);

            var children = (await config.GetChildrenAsync()).ToArray();
            Assert.Equal(5, children.Length);
            var loggingSection = await config.GetSectionAsync("Logging");
            Assert.Equal("Logging", loggingSection!.Path);
            var loggingChildren = (await loggingSection.GetChildrenAsync()).ToArray();
            Assert.Single(loggingChildren);
            loggingSection = loggingChildren[0] as IConfigurationSection;
            Assert.NotNull(loggingSection);
            Assert.Equal("Logging:LogLevel", loggingSection!.Path);

            var stringValue = await config.GetAsync<string>("TestString");
            Assert.NotNull(stringValue);
            Assert.Equal("Hello World!", stringValue);

            var stringArray = await config.GetAsync<string[]>("TestStringArray");
            Assert.NotNull(stringArray);
            Assert.Equal(3, stringArray!.Length);
            
            var numArray = await config.GetAsync<double[]>("TestIntArray");
            Assert.NotNull(numArray);
            Assert.Equal(2, numArray!.Length);

            var logLevel = await config.GetAsync<IConfigurationSectionExtended>("Logging:LogLevel");
            Assert.Equal("Logging:LogLevel", logLevel!.Path);
            Assert.NotNull(logLevel);
            Assert.Equal(3, logLevel.Count);
        }

        [Fact]
        public async Task Overload_config_from_folder_and_assert_values()
        {
            var folder = new DirectoryInfo("../../../_files");
            var environmentResolver = XpServices.GetRequired<IRuntimeEnvironmentResolver>();
            var loader = new ConfigurationLoader(environmentResolver);
            var config = await loader.LoadFromAsync(folder, null, RuntimeEnvironment.Sandbox);
            
            var children = (await config.GetChildrenAsync()).ToArray();
            Assert.Equal(6, children.Length);
            var value = await config.GetAsync<string>("TestString");
            Assert.Equal("Hello World!", value);
            value = await config.GetAsync<string>("NewTestString");
            Assert.Equal("New!", value);
            
            var loggingSection = await config.GetSectionAsync("Logging");
            Assert.Equal("Logging", loggingSection!.Path);
            var loggingChildren = (await loggingSection.GetChildrenAsync()).ToArray();
            Assert.Single(loggingChildren);
            loggingSection = loggingChildren[0] as IConfigurationSection;
            Assert.NotNull(loggingSection);
            Assert.Equal("Logging:LogLevel", loggingSection!.Path);
            var defaultLog = await loggingSection.GetAsync<string>("Default");
            Assert.NotNull(defaultLog);
            Assert.Equal("Trace", defaultLog);

            var stringValue = await config.GetAsync<string>("TestString");
            Assert.NotNull(stringValue);
            Assert.Equal("Hello World!", stringValue);

            var stringArray = await config.GetAsync<string[]>("TestStringArray");
            Assert.NotNull(stringArray);
            Assert.Equal(3, stringArray!.Length);
            
            var numArray = await config.GetAsync<double[]>("TestIntArray");
            Assert.NotNull(numArray);
            Assert.Equal(4, numArray!.Length);
        }

        public ConfigurationSectionTests()
        {
            XpServices.BuildFor().Desktop().Include(typeof(ConfigurationSection)).BuildXpServices();
        }
    }
}