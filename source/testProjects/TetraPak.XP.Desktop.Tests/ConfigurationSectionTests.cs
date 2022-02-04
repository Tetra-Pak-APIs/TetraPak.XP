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
            const string SettingsFile = "../../../_files/appsettings.basic.json";
            var config = await ConfigurationLoader.LoadFromAsync(SettingsFile); 
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

            var logLevel = await config.GetAsync<IConfigurationSectionExtended>("Logging:LogLevel");
            Assert.NotNull(logLevel);
            Assert.Equal(3, logLevel!.Count);
        }

        public ConfigurationSectionTests()
        {
            XpServices.Include(typeof(ConfigurationSection)).BuildXpServices();
        }
    }

    
}