using System;
using System.Collections.Generic;
using System.Linq;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Represents the result of an authorization operation.
    /// </summary>
    // ReSharper disable once ClassCanBeSealed.Global
    public class Grant // todo make Grant serializable
    {
        readonly Dictionary<string, object> _tags = new();

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public static TimeSpan SubtractFromExpires { get; set; } = TimeSpan.FromSeconds(2); // todo make configurable?
        
        /// <summary>
        ///   A collection of tokens (represented as <see cref="TokenInfo"/> objects) returned from the issuer.
        /// </summary>
        public TokenInfo[]? Tokens { get; }

        /// <summary>
        ///   Gets the access token when successful.
        /// </summary>
        public ActorToken? AccessToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.Token;

        /// <summary>
        ///   Gets an optional refresh token when successful.
        /// </summary>
        public ActorToken? RefreshToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.RefreshToken)?.Token;

        /// <summary>
        ///   Gets an optional identity token when successful.
        /// </summary>
        public ActorToken? IdToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.IdToken)?.Token;

        /// <summary>
        ///   Gets any provided expiration time when successful.
        /// </summary>
        public DateTime? Expires => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.Expires;

        /// <summary>
        ///   Gets the granted scope as a <see cref="MultiStringValue"/>.
        /// </summary>
        public MultiStringValue? Scope { get; set; }
        
        internal T? GetValue<T>(string key, T? useDefault) 
            =>
            _tags.TryGetValue(key, out var obj) && obj is T tv ? tv : useDefault;

        internal void SetValue(string key, object value) => _tags[key] = value;

        internal void SetFlag(string key) => _tags[key] = true;

        internal bool IsFlagSet(string key) => GetValue(key, false);

        internal Grant Clone(TimeSpan remainingLifeSpan) 
        {
            var tokens = new List<TokenInfo>();
            if (!(Tokens?.Any() ?? false))
                return new Grant(Array.Empty<TokenInfo>());
                
            foreach (var token in Tokens)
            {
                if (token.Role == TokenRole.AccessToken)
                {
                    tokens.Add(token.Clone(DateTime.UtcNow.Add(remainingLifeSpan.Subtract(SubtractFromExpires))));
                    continue;
                }
                tokens.Add(token.Clone(null));
            }

            return new Grant(tokens.ToArray());
        }
        
        /// <summary>
        ///   Gets a value indicating whether the <see cref="Grant"/> is expired. 
        /// </summary>
        public bool IsExpired => Expires <= DateTime.UtcNow;

        public Grant(params TokenInfo[] tokens)
        { 
            Tokens = tokens;
        }
    }
}