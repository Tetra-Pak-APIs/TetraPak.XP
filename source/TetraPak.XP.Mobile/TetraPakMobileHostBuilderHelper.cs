using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.DependencyInjection;

namespace TetraPak.XP.Mobile
{
    public static class TetraPakMobileHostBuilderHelper
    {
        public static TetraPakHostInfo BuildTetraPakMobileHost(
            this Xamarin.Forms.Application application,
            Action<IServiceCollection>? configureServices = null)
        {
            var collection = XpServices.BuildFor().Mobile().WithServiceCollection(new ServiceCollection());

            // configuration support ...
            var sharedAssembly = application.GetType().Assembly;
            var resourceNames = sharedAssembly.GetManifestResourceNames();
            var appSettingsResourceNames = new List<string>();
            foreach (var resourceName in resourceNames)
            {
                if (resourceName.Contains("appsettings") && resourceName.EndsWith(".json"))
                {
                    appSettingsResourceNames.Add(resourceName);
                }
            }

            if (appSettingsResourceNames.Any())
            {
                var configurationBuilder = new ConfigurationBuilder();
                foreach (var resourceName in appSettingsResourceNames)
                {
                    var resourceStream = sharedAssembly.GetManifestResourceStream(resourceName);
                    configurationBuilder.AddJsonStream(resourceStream);
                }
                var conf = configurationBuilder.Build();
                collection.AddSingleton<IConfiguration>(conf);
            }
            collection.AddTetraPakConfiguration();
            configureServices?.Invoke(collection);
            return new TetraPakHostInfo(/*host, obsolete */ collection);
            
            // var host = Host.CreateDefaultBuilder(args)
            //     .ConfigureServices(collection =>
            //     {
            //         collection =
            //             XpServices
            //                 .BuildFor().Mobile().WithServiceCollection(collection)
            //                 .AddTetraPakConfiguration();
            //         configureServices?.Invoke(collection);
            //         tcs.SetResult(collection);
            //     })
            //     .ConfigureHostConfiguration(builder => { builder.AddEnvironmentVariables(); })
            //     .ConfigureAppConfiguration((_, builder) => builder.Build())
            //     .Build();
            //
            // var collection = tcs.Task.Result;
            // return new TetraPakHostInfo(/*host, obsolete */ collection);
        }

    }
    
    public sealed class TetraPakHostInfo
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        //public IHost Host { get; }

        public IServiceCollection ServiceCollection { get; }

        internal TetraPakHostInfo(IServiceCollection serviceCollectionCollection)
        {
            // Host = host;
            ServiceCollection = serviceCollectionCollection;
        }
    }
}