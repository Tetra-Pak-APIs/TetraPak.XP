using System.Threading;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Used to describe an auth request context.
    /// </summary>
    /// <seealso cref="TetraPakConfig.ConfigDelegate"/>
    public class AuthContext
    {
        /// <summary>
        ///   Specifies the requested <see cref="GrantType"/>.
        /// </summary>
        public GrantType GrantType { get; }

        /// <summary>
        ///   Gets the <see cref="IServiceAuthConfig"/> object.
        /// </summary>
        public IServiceAuthConfig AuthConfig { get; }

        public GrantOptions Options { get; }

        public CancellationToken CancellationToken => Options.CancellationTokenSource?.Token ?? CancellationToken.None;
        
        /// <summary>
        ///   Initializes the <see cref="AuthContext"/>.
        /// </summary>
        /// <param name="grantType">
        ///   Initializes <see cref="GrantType"/>.
        /// </param>
        /// <param name="authConfig">
        ///   Initializes <see cref="AuthConfig"/>. 
        /// </param>
        public AuthContext(GrantType grantType, IServiceAuthConfig authConfig, GrantOptions options)
        {
            GrantType = grantType;
            AuthConfig = authConfig;
            Options = options;
        }
    }
}