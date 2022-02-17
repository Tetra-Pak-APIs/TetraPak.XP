using System;
using System.Threading;
using System.Threading.Tasks;

namespace TetraPak.XP.Auth.AuthCode
{
    
    /// <summary>
    /// todo This is the first step in making the "TetraPakAuthenticator" (from the "TAX" mobile apps lib)
    ///      a service, just like <see cref="IDeviceCodeGrantService"/> and <see cref="IClientCredentialsGrantService"/> 
    /// </summary>
    public interface IAuthorizationCodeGrantService 
    {
        /// <summary>
        ///   Requests a token using the OAuth Device Code grant.   
        /// </summary>
        /// <param name="cancellationTokenSource">
        ///   (optional)<br/>
        ///   Allows canceling the grant request.
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
            CancellationTokenSource? cancellationTokenSource = null,
            MultiStringValue? scope = null, 
            bool forceAuthorization = false);
    }
}