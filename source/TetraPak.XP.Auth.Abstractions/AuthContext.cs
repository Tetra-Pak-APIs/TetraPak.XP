using System.Threading;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Used to describe an auth request context.
    /// </summary>
    public sealed class AuthContext
    {
        /// <summary>
        ///   Gets the <see cref="IAuthConfiguration"/> object.
        /// </summary>
        public IAuthConfiguration? Configuration { get; }

        public GrantOptions Options { get; }
        
        public GrantType GrantType { get; }

        public CancellationToken CancellationToken => Options.CancellationTokenSource?.Token ?? CancellationToken.None;

        /// <summary>
        ///   Initializes the <see cref="AuthContext"/>.
        /// </summary>
        /// <param name="grantType">
        ///     Initializes <see cref="GrantType"/>.
        /// </param>
        /// <param name="options">
        ///     Specifies options for the ongoing <see cref="Grant"/> request.
        /// </param>
        /// <param name="configuration">
        ///     Initializes <see cref="Configuration"/>. 
        /// </param>
        internal AuthContext(GrantType grantType, GrantOptions options, IAuthConfiguration? configuration)
        {
            GrantType = grantType;
            Options = options.WithAuthInfo(configuration);
            Configuration = configuration;
        }
    }
}