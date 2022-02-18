using System;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Auth
{
    public class GrantResponse
    {
        public ActorToken AccessToken { get; private set; }

        public TimeSpan ExpiresIn { get; private set; }

        public MultiStringValue Scope { get; private set; }

        public ActorToken? RefreshToken { get; set; }

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
                response.RefreshToken = RefreshToken;
                return response;
            }
                
            if (typeof(T) == typeof(GrantResponse))
                return (T) new GrantResponse(AccessToken, expiresIn, Scope, RefreshToken);

            response = Activator.CreateInstance<T>();
            response.AccessToken = AccessToken;
            response.ExpiresIn = expiresIn;
            response.Scope = Scope;
            response.RefreshToken = RefreshToken;
            return response;
        }

        public GrantResponse(ActorToken accessToken, TimeSpan expiresIn, MultiStringValue? scope, ActorToken? refreshToken)
        {
            AccessToken = accessToken;
            ExpiresIn = expiresIn;
            Scope = scope ?? MultiStringValue.Empty;
            RefreshToken = refreshToken;
        }
    }
}