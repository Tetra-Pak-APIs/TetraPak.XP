using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Web
{
    /// <summary>
    ///   Provides convenient methods for registering a <see cref="IHttpClientProvider"/>.
    /// </summary>
    public static class HttpClientHelper
    {
        static readonly object s_syncRoot = new();
        static bool s_isTetraPackHttpClientProviderAdded;
        static IHttpClientProvider? s_singleton;
        
        /// <summary>
        ///   Registers a default Tetra Pak <see cref="IHttpClientProvider"/> implementation.
        /// </summary>
        /// <param name="services">
        ///   The service collection.
        /// </param>
        /// <returns>
        ///   The <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddTetraPakHttpClientProvider(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var tetraPakConfig = provider.GetRequiredService<TetraPakConfig>();
            services.AddTetraPakHttpClientProvider(p =>
            {
                if (s_singleton is { })
                    return s_singleton;

                s_singleton = new TetraPakHttpClientProvider(
                    tetraPakConfig
                    ,p.GetService<IAuthorizationService>());
                return s_singleton;
            });
            TetraPakHttpClientProvider.AddDecorator(new TetraPakSdkClientDecorator(tetraPakConfig));
            return services;
        }

        /// <summary>
        ///   Adds a custom <see cref="IHttpClientProvider"/> factory.
        /// </summary>
        /// <param name="services">
        ///   The service collection.
        /// </param>
        /// <param name="factory">
        ///   The <see cref="IHttpClientProvider"/> factory, used to produce <see cref="HttpClient"/>a.
        /// </param>
        /// <returns>
        ///   The <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddTetraPakHttpClientProvider(
            this IServiceCollection services,
            Func<IServiceProvider, IHttpClientProvider> factory)
        {
            lock (s_syncRoot)
            {
                if (s_isTetraPackHttpClientProviderAdded)
                    return services;

                try
                {
                    services.AddSingleton(factory);
                    s_isTetraPackHttpClientProviderAdded = true;
                }
                catch (Exception ex)
                {
                    var p = services.BuildServiceProvider();
                    var logger = p.GetService<ILog>();
                    logger.Error(ex, "Failed to register Tetra Pak HTTP client provider (from factory) with service collection");
                }
                return services;
            }
        }
    }
}