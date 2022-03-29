using System;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Identity
{
    public interface IUserInformationService
    {
        /// <summary>
        ///   Obtains (and, optionally, caches) user information. 
        /// </summary>
        /// <param name="grant">
        ///   A <see cref="Grant"/> identifying and authenticating the requesting actor. 
        /// </param>
        /// <param name="options">
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking a request/response (mainly for diagnostics purposes).
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, carry
        ///   a <see cref="UserInformation"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        Task<Outcome<UserInformation>> GetUserInformationAsync(
            Grant grant,
            GrantOptions options,
            LogMessageId? messageId = null);
    }
}