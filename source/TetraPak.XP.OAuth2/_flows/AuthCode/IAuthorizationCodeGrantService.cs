using System;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2.AuthCode
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