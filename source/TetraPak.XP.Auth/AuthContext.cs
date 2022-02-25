using System.Threading;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   Used to describe an auth request context.
    /// </summary>
    public class AuthContext : ServiceAuthConfigSectionWrapper
    {
        readonly GrantOptions _options;
        readonly GrantType? _grantType;

        public override GrantType GrantType => _grantType ?? base.GrantType;

        /// <summary>
        ///   Gets the <see cref="IServiceAuthConfig"/> object.
        /// </summary>
        public IServiceAuthConfig AuthConfig { get; }

        public CancellationToken CancellationToken => _options.CancellationTokenSource?.Token ?? CancellationToken.None;

        /// <summary>
        ///   Initializes the <see cref="AuthContext"/>.
        /// </summary>
        /// <param name="grantType">
        ///   Initializes <see cref="GrantType"/>.
        /// </param>
        /// <param name="authConfig">
        ///   Initializes <see cref="AuthConfig"/>. 
        /// </param>
        /// <param name="options">
        ///   Specifies options for the ongoing <see cref="Grant"/> request.
        /// </param>
        public AuthContext(GrantType grantType, IServiceAuthConfig authConfig, GrantOptions options)
        : base(authConfig)
        {
            _grantType = grantType;
            AuthConfig = authConfig;
            _options = options;
        }
    }
}