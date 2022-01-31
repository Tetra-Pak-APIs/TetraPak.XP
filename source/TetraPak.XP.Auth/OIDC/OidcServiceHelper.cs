using System;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.SimpleDI;

namespace TetraPak.XP.Auth.OIDC
{
    public static class ServiceCollectionHelper
    {
        public static IServiceCollection AddTetraPakOidcAuthentication(
            this IServiceCollection services,
            AuthApplication authApplication)
        {
            services.RegisterAllXpDependencies();
            throw new NotImplementedException(); // todo add OIDC services
            return services;
        }
        
        public static IServiceCollection AddTetraPakOidcAuthentication(
            this IServiceCollection services,
            RuntimeEnvironment environment,
            string clientId,
            Uri redirectUri,
            RuntimePlatform runtimePlatform = RuntimePlatform.Any)
        {
            return services.AddTetraPakOidcAuthentication(
                new AuthApplication(
                    clientId,
                    redirectUri, 
                    environment,
                    runtimePlatform));
        }

    }
}