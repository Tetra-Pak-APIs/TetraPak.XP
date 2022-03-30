using System;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Classes implementing this contract can be used to obtain application credentials (client id/client secret)
    ///   from an <see cref="AuthContext"/>.
    /// </summary>
    public interface IAppCredentialsDelegate
    {
        /// <summary>
        ///   Resolves and returns application credentials (client id/client secret) from an <see cref="AuthContext"/>.
        /// </summary>
        /// <param name="context">
        ///   The authorization context to obtain application credentials for. 
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="Credentials"/> or, on failure, <see cref="Outcome.Exception"/> and <see cref="Outcome.Message"/>.
        /// </returns>
        Outcome<Credentials> GetAppCredentials(AuthContext context);
    }
}