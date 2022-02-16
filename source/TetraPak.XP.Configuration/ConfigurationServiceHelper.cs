using System.IO;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.DependencyInjection;

namespace TetraPak.XP.Configuration
{
    public static class ConfigurationServiceHelper
    {
        static bool s_isConfigurationAdded;
        static readonly object s_syncRoot = new();
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds a <see cref="IConfiguration"/> service to the <see cref="IServiceCollection"/>
        ///   and then returns the service collection.
        /// </summary>
        /// <param name="collection">
        ///   The service collection.
        /// </param>
        /// <param name="folder">
        ///   (optional; default=current folder)<br/>
        ///   A folder to read configuration from.
        /// </param>
        /// <returns>
        ///   The service <paramref name="collection"/>.
        /// </returns>
        public static IServiceCollection AddConfiguration(
            this IServiceCollection collection, 
            DirectoryInfo? folder = null)
        {
            lock (s_syncRoot)
            {
                if (s_isConfigurationAdded)
                    return collection;

                s_isConfigurationAdded = true;
            }

            collection.RegisterXpServices();
            collection.AddSingleton<IConfiguration>(p =>
            {
                var environmentResolver = p.GetRequiredService<IRuntimeEnvironmentResolver>();
                var configurationLoader = new ConfigurationLoader(environmentResolver);
                var configuration = configurationLoader.LoadFromAsync(folder).Result;
#if NET5_0_OR_GREATER
                return configuration;
#else
                return configuration!;
#endif
            });
            
            return collection;
        }
    }
}