using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Web.Http
{
    public static class HttpServiceHelper
    {
        static bool s_isHttpClientProviderAdded;
        static readonly object s_syncRoot = new();

        public static IServiceCollection UseTetraPakHttpClientProvider(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isHttpClientProviderAdded)
                    return collection;

                s_isHttpClientProviderAdded = true;
            }

            collection.UseTetraPakConfiguration();
            collection.AddSingleton<IHttpClientProvider>(p =>
            {
                var config = p.GetRequiredService<ITetraPakConfiguration>();
                var authService = p.GetService<IAuthorizationService>();
                var log = p.GetService<ILog>();
                TetraPakHttpClientProvider.AddDecorator(new TetraPakMessageIdClientDecorator());
                return new TetraPakHttpClientProvider(config, authService, log);
            });

            return collection;
        }
    }
}