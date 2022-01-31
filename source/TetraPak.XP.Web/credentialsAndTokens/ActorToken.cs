using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

#nullable enable

namespace TetraPak
{
    /// <summary>
    ///   Security token issued for an actor.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]

    public class ActorToken : Credentials, IStringValue
    {
        bool? _isJwt;
        
        /// <summary>
        ///   The actor's identity.
        /// </summary>
        public string Value => Identity;

        /// <inheritdoc />
        public string StringValue => Value;

        /// <inheritdoc />
        public override string ToString() => StringValue;

        /// <summary>
        ///   Returns a value to indicate whether the actor token is assigned.  
        /// </summary>
        public override bool IsAssigned => !string.IsNullOrWhiteSpace(Identity);

        /// <summary>
        ///   Returns a value to indicate the actor token is a JWT.  
        /// </summary>
        public bool IsJwt => _isJwt ?? (_isJwt = checkIsJwt()).Value;

        /// <summary>
        ///   Attempts parsing the value. 
        /// </summary>
        /// <param name="value">
        ///   The value to be parsed.
        /// </param>
        /// <param name="identity">
        ///   Passes back the identity (token).
        /// </param>
        /// <returns>
        ///   <c>true</c> if parsing was successful; otherwise <c>false</c>.
        /// </returns>
        protected virtual bool OnTryParse(string value, [NotNullWhen(true)] out string? identity)
        {
            identity = value;
            return true;
        }

        /// <summary>
        ///   Converts the string representation of a token.
        ///   A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="s">
        ///   A string containing a token to convert.
        /// </param>
        /// <param name="token">
        ///   When this method returns, contains the token equivalent of the value contained in <paramref name="s"/>,
        ///   if the conversion succeeded, or null if the conversion failed.
        ///   The conversion fails if the s parameter is null or Empty, or is not of the correct format.
        ///   This parameter is passed uninitialized; any value originally supplied in result will be overwritten.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParse(string s, [NotNullWhen(true)] out ActorToken? token)
        {
            if (BearerToken.TryParse(s, out var bearerToken))
            {
                token = bearerToken;
                return true;
            }
            
            token = null;
            if (string.IsNullOrWhiteSpace(s))
                return false;

            token = new ActorToken(s, false);
            return true;
        }
        
        bool tryParse(string value, [NotNullWhen(true)] out string? identity) => OnTryParse(value, out identity);

        /// <summary>
        ///   Implicitly casts an <see cref="ActorToken"/> to its textual representation. 
        /// </summary>
        public static implicit operator string?(ActorToken? token) => token?.ToString();
        
        /// <summary>
        ///   Implicitly casts a token's textual representation into a <see cref="ActorToken"/>.
        /// </summary>
        public static implicit operator ActorToken?(string? stringValue) 
            => string.IsNullOrWhiteSpace(stringValue)
                ? null
                : new ActorToken(stringValue);

        /// <summary>
        ///   Returns the token as a <see cref="ToJwtSecurityToken"/> (if applicable).
        /// </summary>
        /// <returns>
        ///   A <see cref="ToJwtSecurityToken"/> if the token has that form; otherwise <c>null</c>.
        /// </returns>
        public JwtSecurityToken? ToJwtSecurityToken()
        {
            if (_isJwt.HasValue && !_isJwt.Value)
                return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                return handler.ReadJwtToken(Identity);
            }
            catch
            {
                return null;
            }
        }
        
        bool checkIsJwt()
        {
            var jwt = ToJwtSecurityToken();
            return jwt is { };
        }

        /// <summary>
        ///   Initializes an (empty/unassigned) <see cref="ActorToken"/>.
        /// </summary>
#if NET5_0_OR_GREATER            
        [JsonConstructor]
#endif
        public ActorToken()
        {
        }

        /// <summary>
        ///   Initializes an <see cref="ActorToken"/> from its textual representation.
        /// </summary>
        /// <param name="stringValue">
        ///   The token's textual representation.
        /// </param>
        /// <param name="parse">
        ///   (optional; default=<c>true</c>)<br/>
        ///   Specifies whether to automatically parse the textual representation.
        /// </param>
        protected ActorToken(string stringValue, bool parse = true) 
        : this()
        {
            if (!parse)
            {
                Identity = stringValue;
                return;
            }
            if (tryParse(stringValue, out var identity))
                Identity = identity;
        }
    }
}