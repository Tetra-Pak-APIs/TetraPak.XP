using TetraPak.XP.Configuration;

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
    
    public class AppCredentialsDelegate : IAppCredentialsDelegate
    {
        public virtual Outcome<Credentials> GetAppCredentials(AuthContext context)
        {
            var identity = context.Configuration?.ClientId;
            if (string.IsNullOrWhiteSpace(identity))
                return Outcome<Credentials>.Fail(
                    new ConfigurationException("Client credentials have not been provisioned"));

            var secret = context.Configuration!.ClientSecret;
            return Outcome<Credentials>.Success(new BasicAuthCredentials(identity!, secret!));
        }
    }
}