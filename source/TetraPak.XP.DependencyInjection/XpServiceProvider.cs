using System;
using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP.DependencyInjection
{
    sealed class XpServiceProvider : IServiceProvider
    {
        readonly IServiceProvider _provider;

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

        internal XpServiceProvider(
            IServiceCollection collection, 
            ServiceProviderOptions options)
        {
            _provider = collection.BuildServiceProvider(options);
        }
    }
}