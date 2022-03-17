using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.DependencyInjection;
using Xunit;

namespace TetraPak.XP.Desktop.Tests
{
    public class ConfigurationSectionTests
    {
        static IServiceProvider initServices(params string[] configFiles)
        {
            return initServices(null, configFiles);
        }

        static IServiceProvider initServices(IRuntimeEnvironmentResolver? environmentResolver, params string[] configFiles)
        {
            IServiceProvider provider = null!;
            Host.CreateDefaultBuilder()
                .ConfigureServices(collection =>
                {
                    if (environmentResolver is {})
                    {
                        provider = XpServices
                            .BuildFor().Desktop().WithServiceCollection(collection)
                            .AddSingleton<IRuntimeEnvironmentResolver>(p => environmentResolver)
                            .AddTetraPakConfiguration()
                            .BuildXpServices();
                    }
                    else
                    {
                        provider = XpServices
                            .BuildFor().Desktop().WithServiceCollection(collection)
                            .AddTetraPakConfiguration()
                            .BuildXpServices();
                    }
                })
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables();
                    foreach (var file in configFiles)
                    {
                        builder.AddJsonFile(file);
                    }
                })
                .ConfigureAppConfiguration((_, builder) => builder.Build())
                .Build();
            
            return provider;
        }
        
        [Fact]
        public async Task Load_config_from_specified_file_and_assert_paths()
        {
            var provider = initServices("../../../_files/appsettings.json");
            
            // var loader = new ConfigurationLoader(environmentResolver);
            // var config = await loader.LoadFromAsync(SettingsFile);
            var config = provider.GetRequiredService<IConfiguration>();

            var subSections = config!.GetSubSections().ToArray();
            Assert.Equal(5, subSections.Length);
            var loggingSection = config.GetSubSection("Logging");
            Assert.Equal("Logging", loggingSection!.Path);
            var loggingChildren = loggingSection.GetChildren().ToArray();
            Assert.Single(loggingChildren);
            loggingSection = loggingChildren[0] as IConfigurationSection;
            Assert.NotNull(loggingSection);
            Assert.Equal("Logging:LogLevel", loggingSection!.Path);

            var stringValue = config.Get<string>("TestString");
            Assert.NotNull(stringValue);
            Assert.Equal("Hello World!", stringValue);

            // var stringArray = config.Get<string[]>("TestStringArray");
            // Assert.NotNull(stringArray);
            // Assert.Equal(3, stringArray!.Length);
            //
            // var numArray = config.Get<double[]>("TestIntArray");
            // Assert.NotNull(numArray);
            // Assert.Equal(2, numArray!.Length);

            var logLevel = config.GetSubSection("Logging:LogLevel");
            Assert.Equal("Logging:LogLevel", logLevel!.Path);
            Assert.NotNull(logLevel);
            // Assert.Equal(3, logLevel.Count);
        }

        [Fact]
        public async Task Overload_config_from_folder_and_assert_values()
        {
            // const string AppSettings = "../../../_files/appsettings.json";
            // const string AppSettingsSandbox = "../../../_files/appsettings.Sandbox.json";
            // var provider = initServices(new SandboxEnvironment(), AppSettings, AppSettingsSandbox);
            // var config = provider.GetRequiredService<IConfiguration>();
            //
            // var children = config.GetSubSections().ToArray();
            // Assert.Equal(6, children.Length);
            // var value = await config.GetAsync<string>("TestString");
            // Assert.Equal("Hello World!", value);
            // value = await config.GetAsync<string>("NewTestString");
            // Assert.Equal("New!", value);
            //
            // var loggingSection = await config.GetSectionAsync("Logging");
            // Assert.Equal("Logging", loggingSection!.Path);
            // var loggingChildren = (await loggingSection.GetChildrenAsync()).ToArray();
            // Assert.Single(loggingChildren);
            // loggingSection = loggingChildren[0] as IConfigurationSection;
            // Assert.NotNull(loggingSection);
            // Assert.Equal("Logging:LogLevel", loggingSection!.Path);
            // var defaultLog = await loggingSection.GetAsync<string>("Default");
            // Assert.NotNull(defaultLog);
            // Assert.Equal("Trace", defaultLog);
            //
            // var stringValue = await config.GetAsync<string>("TestString");
            // Assert.NotNull(stringValue);
            // Assert.Equal("Hello World!", stringValue);
            //
            // var stringArray = await config.GetAsync<string[]>("TestStringArray");
            // Assert.NotNull(stringArray);
            // Assert.Equal(3, stringArray!.Length);
            //
            // var numArray = await config.GetAsync<double[]>("TestIntArray");
            // Assert.NotNull(numArray);
            // Assert.Equal(4, numArray!.Length);
        }

        public ConfigurationSectionTests()
        {
            XpServices.BuildFor().Desktop().Include(typeof(ConfigurationSection)).BuildXpServices();
        }

        class SandboxEnvironment : IRuntimeEnvironmentResolver
        {
            public RuntimeEnvironment ResolveRuntimeEnvironment(RuntimeEnvironment useDefault = RuntimeEnvironment.Production)
            {
                return RuntimeEnvironment.Sandbox;
            }
        }
    }
}