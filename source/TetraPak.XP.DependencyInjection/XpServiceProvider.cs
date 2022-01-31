using System;
using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP.SimpleDI
{
    class XpServiceProvider : IServiceProvider
    {
        readonly ServiceProvider _provider;
        
        public object GetService(Type serviceType)
        {
            var xpDependencyOutcome = XpServices.TryGet(serviceType);
            return xpDependencyOutcome 
                ? xpDependencyOutcome.Value! 
                : _provider.GetService(serviceType);
        }

        public XpServiceProvider(IServiceCollection services, ServiceProviderOptions options)
        {
            _provider = services.BuildServiceProvider(options);
        }
    }
}