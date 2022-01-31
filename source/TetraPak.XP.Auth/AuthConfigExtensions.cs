using System;
using System.Collections.Generic;
using System.Linq;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   Offers convenient extensions for the <see cref="AuthConfig"/> class.
    /// </summary>
    public static class AuthConfigExtensions
    {
        /// <summary>
        ///   (fluent api)<br/>
        ///   Assigns the <see cref="AuthConfig.Authority"/> and returns the <see cref="AuthConfig"/>. 
        /// </summary>
        public static AuthConfig WithAuthority(this AuthConfig self, Uri authorityUri)
        {
            self.Authority = authorityUri;
            return self;
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Assigns the <see cref="AuthConfig.TokenIssuer"/> and returns the <see cref="AuthConfig"/>. 
        /// </summary>
        public static AuthConfig WithTokenIssuer(this AuthConfig self, Uri tokenIssuer)
        {
            self.TokenIssuer = tokenIssuer;
            return self;
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Assigns the two endpoint properties
        ///   (<see cref="AuthConfig.Authority"/> and <see cref="AuthConfig.TokenIssuer"/>)
        ///   and returns the <see cref="AuthConfig"/>. 
        /// </summary>
        public static AuthConfig WithEndpoints(this AuthConfig self, Uri authorityUri, Uri tokenIssuer)
        {
            return self.WithAuthority(authorityUri).WithTokenIssuer(tokenIssuer);
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds one or more scope identifiers to the <see cref="AuthConfig.Scope"/> value
        ///   and then returns the <see cref="AuthConfig"/>.
        /// </summary>
        public static AuthConfig WithScope(this AuthConfig self, AuthScope scope)
        {
            self.Scope = scope;
            return self;
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Clears the <see cref="AuthConfig.Scope"/> value anf then returns the <see cref="AuthConfig"/>.
        /// </summary>
        public static AuthConfig ClearScope(this AuthConfig self)
        {
            self.Scope = null;
            return self;
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds one or more scope types (not already supported) to the <see cref="AuthConfig.Scope"/> value
        ///   and then returns the <see cref="AuthConfig"/>.
        /// </summary>
        public static AuthConfig AddScope(this AuthConfig self, params string[] scopeTypes)
        {
            var list = new List<string>(self.Scope.Items);
            foreach (var scope in scopeTypes)
            {
                if (!list.Any(i => i.Equals(scope, StringComparison.InvariantCultureIgnoreCase)))
                    list.Add(scope);
            }
            self.Scope = new AuthScope(list.ToArray());
            return self;
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds one or more scope types (not already supported) to the <see cref="AuthConfig.Scope"/> value
        ///   and then returns the <see cref="AuthConfig"/>.
        /// </summary>
        public static AuthConfig AddScope(this AuthConfig self, AuthScope scope) => self.AddScope(scope.Items);

        /// <summary>
        ///   (fluent api)<br/>
        ///   Removes one or more scope types from the <see cref="AuthConfig.Scope"/> value
        ///   and then returns the <see cref="AuthConfig"/>.
        /// </summary>
        public static AuthConfig RemoveScope(this AuthConfig self, params string[] scopeTypes)
        {
            IEnumerable<string> list = new List<string>(self.Scope.Items);
            foreach (var type in scopeTypes)
            {
                var index = list.IndexOf(i => i.Equals(type, StringComparison.InvariantCultureIgnoreCase));
                if (index != -1)
                    ((List<string>)list).RemoveAt(index);
            }

            return self.WithScope(new AuthScope(list.ToArray()));
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Removes one or more scope types from the <see cref="AuthConfig.Scope"/> value
        ///   and then returns the <see cref="AuthConfig"/>.
        /// </summary>
        public static AuthConfig RemoveScope(this AuthConfig self, AuthScope scope) => self.RemoveScope(scope.Items);

        /// <summary>
        ///   (fluent api)<br/>
        ///   Replaces the default <see cref="AuthConfig.Cache"/> with a (custom) one
        ///   and then returns the <see cref="AuthConfig"/>.
        /// </summary>
        public static AuthConfig WithCache(this AuthConfig self, ITimeLimitedRepositories? cache)
        {
            self.Cache = cache;
            return self;
        }
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Replaces the default <see cref="TokenCache"/> with a (custom) one
        ///   and then returns the <see cref="AuthConfig"/>.
        /// </summary>
        public static AuthConfig WithTokenCache(this AuthConfig self, ITokenCache? tokenCache)
        {
            self.TokenCache = tokenCache;
            return self;
        }

    }
}