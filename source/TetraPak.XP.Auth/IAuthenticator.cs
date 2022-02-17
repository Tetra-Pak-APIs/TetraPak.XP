using System.Threading;
using System.Threading.Tasks;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   Describes the contract for an authenticator implementation.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        ///   Attempts to acquire an access token.
        /// </summary>
        /// <param name="allowCached">
        ///     (optional; default = <c>true</c>)<br/>
        ///     Can be used to override global cache setting
        ///     value for this particular operation.
        /// </param>
        /// <param name="cancellationTokenSource">
        ///   Allows canceling the grant request.
        /// </param>
        /// <returns>
        ///   A <seealso cref="Outcome{T}"/> indicating success while also carrying
        ///   details for the authentication result, including access token.
        /// </returns>
        Task<Outcome<Grant>> GetAccessTokenAsync(bool allowCached = true, CancellationTokenSource? cancellationTokenSource = null);

        /// <summary>
        ///   Attempts to acquire an access token "silently", automatically
        ///   using any persisted refresh token in the process.
        /// </summary>
        /// <param name="cancellationTokenSource">
        ///   Allows canceling the grant request.
        /// </param>
        /// <returns>
        ///   A <seealso cref="Outcome{T}"/> indicating success while also carrying
        ///   details for the authentication result, including access token.
        /// </returns>
        Task<Outcome<Grant>> GetAccessTokenSilentlyAsync(CancellationTokenSource? cancellationTokenSource = null);
    }
}
