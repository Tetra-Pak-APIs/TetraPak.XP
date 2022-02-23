using System;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Auth.Refresh
{
    public interface IRefreshTokenGrantService
    {
        /// <summary>
        ///   Requests a new token from a refresh token, using the OAuth Refresh Token grant.   
        /// </summary>
        /// <param name="refreshToken">
        ///   The refresh token to be passed for a new access token.
        /// </param>
        /// <param name="options">
        ///   Specifies the details for how to perform the grant request.
        /// </param>
        /// <returns>
        ///   An <see cref="Exception"/> instance indicating success/failure, and the requested token
        ///   when successful; otherwise an <see cref="Outcome"/>.
        /// </returns>
        Task<Outcome<Grant>> AcquireTokenAsync(ActorToken refreshToken, GrantOptions options);
    }
}