using System;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.ClientCredentials;

namespace TetraPak.XP.Auth
{
    public class GrantResponse
    {
        public ActorToken AccessToken { get; private set; }

        public TimeSpan ExpiresIn { get; private set; }

        public MultiStringValue Scope { get; private set; }

        public T Clone<T>(TimeSpan expiresIn, Func<T>? factory = null) 
        where T : GrantResponse
        {
            T response;
            if (factory is { })
            {
                response = factory();
                response.AccessToken = AccessToken;
                response.ExpiresIn = expiresIn;
                response.Scope = Scope;
                return response;
            }
                
            if (typeof(T) == typeof(GrantResponse))
                return (T) new GrantResponse(AccessToken, expiresIn, Scope);

            response = Activator.CreateInstance<T>();
            response.AccessToken = AccessToken;
            response.ExpiresIn = expiresIn;
            response.Scope = Scope;
            return response;
        }

        public GrantResponse(ActorToken accessToken, TimeSpan expiresIn, MultiStringValue? scope)
        {
            AccessToken = accessToken;
            ExpiresIn = expiresIn;
            Scope = scope ?? MultiStringValue.Empty;
        }
    }
}