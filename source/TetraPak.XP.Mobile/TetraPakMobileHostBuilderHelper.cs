using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.DependencyInjection;

namespace TetraPak.XP.Mobile
{
    /// <summary>
    ///   Provides convenient helper methods for building a Tetra Pak mobile app host.  
    /// </summary>
    public static class TetraPakMobileHostBuilderHelper
    {
        /// <summary>
        ///   Builds and configures a host for use with a desktop app.
        /// </summary>
        /// <param name="application">
        ///   The (shared) Xamarin.Forms <see cref="Xamarin.Forms.Application"/> object.
        /// </param>
        /// <param name="configureServices">
        ///   (optional)<br/>
        ///   Delegate for configuring custom services with the provided <see cref="IServiceCollection"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="TetraPakHostInfo"/> object.
        /// </returns>
        public static TetraPakHostInfo BuildTetraPakMobileHost(
            this Xamarin.Forms.Application application,
            Action<IServiceCollection>? configureServices = null)
        {
            var collection = XpServices.BuildFor().Mobile()
                .WithServiceCollection(new ServiceCollection())
                .addJsonConfiguration(application)
                .AddTetraPakConfiguration();
            configureServices?.Invoke(collection);
            return new TetraPakHostInfo(collection);
        }

        static IServiceCollection addJsonConfiguration(
            this IServiceCollection collection, 
            Xamarin.Forms.Application application)
        {
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

            return collection;
        }        
    }
    
    /// <summary>
    ///   Provides the result from building a Tetra Pak mobile app host.
    /// </summary>
    public sealed class TetraPakHostInfo
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        //public IHost Host { get; }

        /// <summary>
        ///   The dependency injection service collection,
        ///   used for configuring services. 
        /// </summary>
        /// <seealso cref="XpServices.BuildXpServices(IServiceCollection)"/>
        public IServiceCollection ServiceCollection { get; }

        internal TetraPakHostInfo(IServiceCollection serviceCollectionCollection)
        {
            // Host = host;
            ServiceCollection = serviceCollectionCollection;
        }
    }
}