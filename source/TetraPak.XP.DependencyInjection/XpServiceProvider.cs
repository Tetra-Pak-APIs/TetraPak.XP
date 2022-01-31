using System;
using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP.DependencyInjection
{
    class XpServiceProvider : IServiceProvider
    {
        readonly ServiceProvider _provider;
        
        public object? GetService(Type serviceType) => GetService(serviceType, true);

        // note tryServiceProvider flag prevents endless recursion
        internal object? GetService(Type serviceType, bool tryXpServices)
        {
            var service = _provider.GetService(serviceType);
            if (service is { })
                return service;

            if (!tryXpServices)
                return null;

            var outcome = XpServices.TryGet(serviceType, false);
            return outcome
                ? outcome.Value
                : null;
        }

        public XpServiceProvider(IServiceCollection services, ServiceProviderOptions options)
        {
            _provider = services.BuildServiceProvider(options);
        }
    }
}