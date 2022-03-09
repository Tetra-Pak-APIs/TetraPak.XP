using System;
using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP.OAuth2
{
    public static class OAuthServiceHelper
    {
        static readonly object s_syncRoot = new();
        static bool s_isAppCredentialsDelegateUsed;
        
        public static IServiceCollection UseAppCredentialsDelegate<T>(this IServiceCollection collection)
        where T : class, IAppCredentialsDelegate 
        {
            lock (s_syncRoot)
            {
                if (s_isAppCredentialsDelegateUsed)
                    throw new InvalidOperationException($"A custom {typeof(IAppCredentialsDelegate)} is already in use");

                s_isAppCredentialsDelegateUsed = true;
            }

            collection.AddSingleton<IAppCredentialsDelegate, T>();
            return collection;
        }
    }
}