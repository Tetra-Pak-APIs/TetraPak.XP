using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Web.Http
{
    /// <summary>
    ///   Provides convenient helper methods for working with HTTP services.
    /// </summary>
    public static class HttpServiceHelper
    {
        static bool s_isHttpClientProviderAdded;
        static readonly object s_syncRoot = new();

        /// <summary>
        ///   (fluent api)<br/>
        ///   Configures a HTTP client provider service for typical Tetra Pak use and returns the service collection.
        /// </summary>
        /// <param name="collection">
        ///   The service collection to be configured.
        /// </param>
        /// <returns>
        ///   The service collection.
        /// </returns>
        public static IServiceCollection AddTetraPakHttpClientProvider(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isHttpClientProviderAdded)
                    return collection;

                s_isHttpClientProviderAdded = true;
            }

            collection.AddTetraPakConfiguration();
            collection.AddSingleton<IHttpClientProvider>(p =>
            {
                var config = p.GetRequiredService<ITetraPakConfiguration>();
                var authService = p.GetService<IAuthorizationService>();
                var log = p.GetService<ILog>();
                TetraPakHttpClientProvider.AddDecorator(new TetraPakMessageIdClientDecorator());
                TetraPakHttpClientProvider.AddDecorator(new TetraPakSdkHeaderClientDecorator());
                return new TetraPakHttpClientProvider(config, authService, log);
            });

            return collection;
        }
    }
}