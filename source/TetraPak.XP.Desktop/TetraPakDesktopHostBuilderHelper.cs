using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.DependencyInjection;

namespace TetraPak.XP.Desktop
{
    public static class TetraPakDesktopHostBuilderHelper
    {
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
                            .AddTetraPakConfiguration();
                    configureServices?.Invoke(collection);
                    tcs.SetResult(collection);
                })
                .ConfigureHostConfiguration(builder => { builder.AddEnvironmentVariables(); })
                .ConfigureAppConfiguration((_, builder) => builder.Build())
                .Build();

            var collection = tcs.Task.Result;
            return new TetraPakHostInfo(host, collection);
        }
    }

    public sealed class TetraPakHostInfo
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IHost Host { get; }

        public IServiceCollection ServiceCollection { get; }

        internal TetraPakHostInfo(IHost host, IServiceCollection serviceCollectionCollection)
        {
            Host = host;
            ServiceCollection = serviceCollectionCollection;
        }
    }
}