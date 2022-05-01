using System;
using System.Threading.Tasks;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Carries an individual token and its meta data.
    /// </summary>
    public class TokenInfo // todo make TokenInfo serializable
    {
        readonly TokenValidationDelegate? _tokenValidationDelegate;
        bool _isValidatedByDelegate;

        /// <summary>
        ///   Gets the actual token as a <see cref="string"/> value.
        /// </summary>
        public ActorToken Token { get; }

        /// <summary>
        ///   Gets the token role (see <see cref="TokenRole"/>).
        /// </summary>
        public TokenRole Role { get; }

        /// <summary>
        ///   Gets a expiration date/time, if available.
        /// </summary>
        public DateTime? Expires { get; }

        internal TokenInfo Clone(DateTime? expires) => new(Token, Role, expires);

        /// <summary>
        ///   Gets a value that indicates whether the token can be validated (other than just by its longevity).
        /// </summary>
        public bool IsValidatable => _tokenValidationDelegate != null;

        /// <summary>
        ///   Validates the token and returns a value to indicate whether it is valid at this point. 
        /// </summary>
        public async Task<bool> IsValidAsync()
        {
            if (isTokenExpired())
                return false;

            if (_tokenValidationDelegate is null || _isValidatedByDelegate)
                return true;

            var isValid = await _tokenValidationDelegate(Token);
            _isValidatedByDelegate = true;
            return isValid;
        }

        bool isTokenExpired() => Expires.HasValue && Expires.Value <= XpDateTime.Now;

        //[JsonConstructor]
        public TokenInfo(
            ActorToken token,
            TokenRole role,
            DateTime? expires)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            Role = role;
            Expires = expires;
        }

        internal TokenInfo(
            ActorToken token,
            TokenRole role,
            DateTime? expires = null,
            TokenValidationDelegate? tokenValidationDelegate = null)
            : this(token, role, expires)
        {
            _tokenValidationDelegate = tokenValidationDelegate;
        }
    }
}