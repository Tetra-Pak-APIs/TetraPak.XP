using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.DependencyInjection;

namespace TetraPak.XP.Desktop
{
    public static class TetraPakDesktopHostBuilderHelper
    {
        static readonly object s_syncRoot = new();
        static bool s_isTokenCacheAdded;
        static bool s_isFileSystemAdded;
        
        /// <summary>
        ///   Builds and configures a host for use with a desktop app.
        /// </summary>
        /// <param name="args">
        ///   A collection of string arguments.
        /// </param>
        /// <param name="configureServices">
        ///   (optional)<br/>
        ///   Delegate for configuring custom services with the provided <see cref="IServiceCollection"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="TetraPakHostInfo"/> object.
        /// </returns>
        public static TetraPakHostInfo BuildTetraPakDesktopHost(
            this string[] args,
            Action<IServiceCollection>? configureServices = null)
        {
            var tcs = new TaskCompletionSource<IServiceCollection>();
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(collection =>
                {
                    collection =
                        XpServices
                            .BuildFor().Desktop().WithServiceCollection(collection)
                            .AddXpDateTime()
                            .AddTetraPakConfiguration()
                            .AddDesktopFileSystem()
                            .AddDesktopTokenCache();
                    configureServices?.Invoke(collection);
                    tcs.SetResult(collection);
                })
                .ConfigureHostConfiguration(builder => { builder.AddEnvironmentVariables(); })
                .ConfigureAppConfiguration((_, builder) => builder.Build())
                .Build();

            Configure.InsertValueDelegate(new ConfigurationVariableValueDelegate());
            var collection = tcs.Task.Result;
            return new TetraPakHostInfo(host, collection);
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
        public static IServiceCollection AddDesktopTokenCache(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isTokenCacheAdded)
                    return collection;

                s_isTokenCacheAdded = true;
            }
            
            collection.AddDataProtection();
            collection.AddTokenCache<DataProtectionTokenCache>();
            return collection;
        }

        /// <summary>
        ///   Adds a token cache service for use with a desktop app.
        /// </summary>
        /// <param name="collection">
        ///   The service collection.
        /// </param>
        /// <param name="cachePath">
        ///   (optional; default=./.cache)<br/>
        ///   Specifies the path to the file cache folder.
        /// </param>
        /// <returns>
        ///   The service <paramref name="collection"/>.
        /// </returns>
        public static IServiceCollection AddDesktopFileSystem(
            this IServiceCollection collection, 
            string cachePath = "./.cache")
        {
            lock (s_syncRoot)
            {
                if (s_isFileSystemAdded)
                    return collection;

                s_isFileSystemAdded = true;
            }
            
            collection.AddSingleton<IFileSystem>(_ => new DesktopFileSystem(cachePath));
            return collection;
        }
    }

    public sealed class TetraPakHostInfo
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public IHost Host { get; }

        public IServiceCollection ServiceCollection { get; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        internal TetraPakHostInfo(IHost host, IServiceCollection serviceCollectionCollection)
        {
            Host = host;
            ServiceCollection = serviceCollectionCollection;
        }
    }
}