using System;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.OAuth2.AuthCode;

namespace TetraPak.XP.OAuth2.DeviceCode
{
    /// <summary>
    ///   Implementors of this interface are able to acquire a token using the
    ///   OAuth Device Code grant. 
    /// </summary>
    public interface IDeviceCodeGrantService : IGrantService
    {
        /// <summary>
        ///   Requests an OAuth2 Device Code grant asynchronously.   
        /// </summary>
        /// <param name="options">
        ///   Specifies the details for how to perform the grant request.
        /// </param>
        /// <param name="verificationUriAsyncHandler">
        ///   A handler to be called back with the requested device code and verification URL.
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome"/> to indicate success/failure and, on success, also carry
        ///   the <see cref="Grant"/> or, on failure, <see cref="Outcome.Exception"/> and <see cref="Outcome.Message"/>.
        /// </returns>
        Task<Outcome<Grant>> AcquireTokenAsync(GrantOptions options, Func<VerificationArgs,Task> verificationUriAsyncHandler);
    }
}