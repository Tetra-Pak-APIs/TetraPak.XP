using System;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Auth.DeviceCode
{
    /// <summary>
    ///   Implementors of this interface are able to acquire a token using the
    ///   OAuth Device Code grant. 
    /// </summary>
    public interface IDeviceCodeGrantService
    {
        /// <summary>
        ///   Requests a token using the OAuth Device Code grant.   
        /// </summary>
        /// <param name="verificationUriHandler">
        ///   A handler to be called back with the requested device code and verification URL.
        /// </param>
        /// <param name="cancellationTokenSource">
        ///   (optional)<br/>
        ///   Allows canceling the grant request.
        /// </param>
        /// <param name="clientCredentials">
        ///   (optional)<br/>
        ///   Specifies client credentials.
        /// </param>
        /// <param name="scope">
        ///   (optional)<br/>
        ///   Scope to be requested for the authorization.
        /// </param>
        /// <param name="forceAuthorization">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies whether to force a new client credentials authorization
        ///   (overriding/replacing any cached authorization). 
        /// </param>
        /// <returns>
        ///   An <see cref="Exception"/> instance indicating success/failure, and the requested token
        ///   when successful; otherwise an <see cref="Outcome"/>.
        /// </returns>
        Task<Outcome<Grant>> AcquireTokenAsync(
            Action<VerificationArgs> verificationUriHandler,
            CancellationTokenSource? cancellationTokenSource = null,
            Credentials? clientCredentials = null,
            MultiStringValue? scope = null, 
            bool forceAuthorization = false);
    }
}