using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Desktop
{
public static class TetraPakDesktopHostBuilderHelper
    {
        
        public static TetraPakHostInfo BuildTetraPakDesktopHost(
            this string[] args, 
            Action<IServiceCollection>? configureServices = null)
        {
            var tcs = new TaskCompletionSource<IServiceCollection>();
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(collection =>
                {
                    XpServices
                        .BuildFor().Desktop().WithServiceCollection(collection)
                        .AddTetraPakConfiguration();
                    configureServices?.Invoke(collection);
                    tcs.SetResult(collection);
                })
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables();
                })
                .ConfigureAppConfiguration((_, builder) => builder.Build())
                .Build();
            
             var collection = tcs.Task.Result;
             return new TetraPakHostInfo(host, collection);
        }
        
        static LogRank resolveLogRank(IServiceProvider p, LogRank useDefault)
        {
            var config = p.GetRequiredService<IConfiguration>();
            var logLevelSection = config.GetSubSection(new ConfigPath(new[] { "Logging", "LogLevel" }));
            if (logLevelSection is null)
                return useDefault;

            var s = logLevelSection.GetNamed<string>("Default");
            if (string.IsNullOrEmpty(s))
                return useDefault;
            
            return s!.TryParseEnum(typeof(LogRank), out var obj) && obj is LogRank logRank
                ? logRank
                : useDefault;
        }
        
    }

    public class TetraPakHostInfo
    {
        public IHost Host { get; }

        public IServiceCollection ServiceServiceCollection { get; }

        internal TetraPakHostInfo(IHost host, IServiceCollection serviceCollection)
        {
            Host = host;
            ServiceServiceCollection = serviceCollection;
        }
    }
}