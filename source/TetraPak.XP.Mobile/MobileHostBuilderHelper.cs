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
    public static class MobileHostBuilderHelper
    {
        static readonly object s_syncRoot = new();
        static bool s_isTokenCacheAdded;

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
        /// <param name="collection">
        ///   (optional)<br/>
        ///   A custom <see cref="IServiceCollection"/> to be used for configuring DI services.  
        /// </param>
        /// <returns>
        ///   A <see cref="HostInfo"/> object.
        /// </returns>
        public static HostInfo BuildTetraPakMobileHost(
            this Xamarin.Forms.Application application,
            Action<IServiceCollection>? configureServices = null, 
            IServiceCollection? collection = null)
        {
            collection = XpServices.BuildFor().Mobile().WithServiceCollection(collection ?? new ServiceCollection())
                .RegisterXpServices()
                .addJsonConfiguration(application)
                .AddTetraPakConfiguration()
                .AddXpDateTime()
                .AddMobileTokenCache();
            configureServices?.Invoke((XpServiceCollection) collection);
            return new HostInfo(collection);
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

            var configurationBuilder = new ConfigurationBuilder();
            if (appSettingsResourceNames.Any())
            {
                foreach (var resourceName in appSettingsResourceNames)
                {
                    var resourceStream = sharedAssembly.GetManifestResourceStream(resourceName);
                    configurationBuilder.AddJsonStream(resourceStream);
                }
            }
            var conf = configurationBuilder.Build();
            collection.AddSingleton<IConfiguration>(conf);

            return collection;
        }
        
        /// <summary>
        ///   Adds a token cache service for use with a desktop app.
        /// </summary>
        /// <param name="collection">
        ///   The service collection.
        /// </param>
        /// <returns>
        ///   The service <paramref name="collection"/>.
        /// </returns>
        public static IServiceCollection AddMobileTokenCache(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isTokenCacheAdded)
                    return collection;

                s_isTokenCacheAdded = true;
            }
            
            collection.AddTokenCache<SecureStoreTokenCache>();
            return collection;
        }
    }
    
    /// <summary>
    ///   Provides the result from building a Tetra Pak mobile app host.
    /// </summary>
    public sealed class HostInfo
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        //public IHost Host { get; }

        /// <summary>
        ///   The dependency injection service collection,
        ///   used for configuring services. 
        /// </summary>
        /// <seealso cref="XpServices.BuildXpServices(IServiceCollection)"/>
        public IServiceCollection ServiceCollection { get; }

        internal HostInfo(IServiceCollection serviceCollectionCollection)
        {
            // Host = host;
            ServiceCollection = serviceCollectionCollection;
        }
    }
}