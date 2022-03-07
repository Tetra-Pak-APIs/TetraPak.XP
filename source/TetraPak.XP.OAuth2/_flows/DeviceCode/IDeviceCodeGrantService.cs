using System;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2.DeviceCode
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
        /// <param name="options">
        ///   Specifies the details for how to perform the grant request.
        /// </param>
        /// <param name="verificationUriHandler">
        ///   A handler to be called back with the requested device code and verification URL.
        /// </param>
        /// <returns>
        ///   An <see cref="Exception"/> instance indicating success/failure, and the requested token
        ///   when successful; otherwise an <see cref="Outcome"/>.
        /// </returns>
        Task<Outcome<Grant>> AcquireTokenAsync(GrantOptions options, Action<VerificationArgs> verificationUriHandler);
    }
}