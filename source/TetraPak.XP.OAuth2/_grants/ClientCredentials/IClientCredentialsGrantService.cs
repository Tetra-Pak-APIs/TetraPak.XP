using System;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.OAuth2.AuthCode;

namespace TetraPak.XP.OAuth2.ClientCredentials
{
    /// <summary>
    ///   Classes implementing this contract allows its clients to acquire an OAuth2 Client Credentials grant. 
    /// </summary>
    public interface IClientCredentialsGrantService : IGrantService
    {
        /// <summary>
        ///   Requests an OAuth2 Client Credentials grant asynchronously.   
        /// </summary>
        /// <param name="options">
        ///   Specifies the details for how to perform the grant request.
        /// </param>
        /// <returns>
        ///   An <see cref="Exception"/> instance indicating success/failure, and the requested token
        ///   when successful; otherwise an <see cref="Outcome"/>.
        /// </returns>
        Task<Outcome<Grant>> AcquireTokenAsync(GrantOptions options);
    }
}