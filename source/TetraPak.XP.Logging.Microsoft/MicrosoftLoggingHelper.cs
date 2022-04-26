using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Logging.Microsoft
{
    public static class MicrosoftLoggingHelper
    {
        static readonly object s_syncRoot = new();
        static bool s_isTetraPakMicrosoftLoggingAdded;

        /// <summary>
        ///   Adds the Microsoft <see cref="ILogger"/>-based logging framework as the implementation for the SDK's
        ///   <see cref="ILog"/> logging abstraction. 
        /// </summary>
        /// <param name="collection">
        ///   The (extended) service collection.
        /// </param>
        /// <param name="formatOptions">
        ///   (optional)<br/>
        ///   Specifies logging formatting options. 
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddMicrosoftLogging(
            this IServiceCollection collection,
            LogFormatOptions? formatOptions = null,
            ILog? log = null)
        {
            lock (s_syncRoot)
            {
                if (s_isTetraPakMicrosoftLoggingAdded)
                    return collection;

                s_isTetraPakMicrosoftLoggingAdded = true;
            }

            collection.AddSingleton<ILog>(p => new MicrosoftLoggerHub(
                p.GetRequiredService<ILogger<MicrosoftLoggerHub>>(),
                log,
                p.GetService<IConfiguration>(),
                formatOptions));

            return collection;
        }
    }
}