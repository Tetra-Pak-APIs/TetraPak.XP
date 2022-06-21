using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP.DependencyInjection
{
    sealed class XpServiceProvider : IServiceProvider
    {
        readonly IServiceProvider _provider;
        readonly XpServiceDelegate[] _serviceDelegates;

        public object? GetService(Type serviceType) => GetService(serviceType, true);

        // note tryServiceProvider flag prevents endless recursion
        internal object? GetService(Type serviceType, bool tryXpServices)
        {
            var service = _provider.GetService(serviceType);
            if (service is { })
                return thruDelegates(serviceType, service);

            if (!tryXpServices)
                return thruDelegates(serviceType, null);

            var outcome = XpServices.TryGet(serviceType, false);
            return thruDelegates(
                serviceType,  
                outcome
                    ? outcome.Value
                    : null);
        }

        object? thruDelegates(Type serviceType, object? service)
        {
            foreach (var serviceDelegate in _serviceDelegates)
            {
                serviceDelegate(this, serviceType, ref service);
            }

            return service;
        }

        internal XpServiceProvider(
            IServiceCollection collection, 
            ServiceProviderOptions options,
            IEnumerable<XpServiceDelegate> serviceDelegates)
        {
            _provider = collection.BuildServiceProvider(options);
            _serviceDelegates = serviceDelegates.ToArray();
        }
    }
}