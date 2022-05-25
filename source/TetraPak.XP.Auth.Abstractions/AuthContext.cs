using System;
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

        /// <summary>
        ///   Provides options for a grant request.
        /// </summary>
        public GrantOptions Options { get; }
        
        /// <summary>
        ///   Specifies the auth grant type.
        /// </summary>
        public GrantType GrantType { get; }

        /// <summary>
        ///   Allows for forced cancellation of an auth operation.
        /// </summary>
        public CancellationToken CancellationToken => Options.CancellationTokenSource?.Token ?? CancellationToken.None;

        /// <summary>
        ///   Gets a value specifying a maximum allowed time for the affected operation to complete, or be cancelled.
        /// </summary>
        public TimeSpan? Timeout => Configuration?.Timeout;

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
            options.Timeout ??= Timeout;
        }
    }
}