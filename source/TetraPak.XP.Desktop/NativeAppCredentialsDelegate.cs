using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;

namespace TetraPak.XP.Desktop
{
    public class NativeAppCredentialsDelegate : IAppCredentialsDelegate
    {
        public virtual Outcome<Credentials> GetAppCredentials(AuthContext context)
        {
            var identity = context.Configuration.ClientId;
            if (string.IsNullOrWhiteSpace(identity))
                return Outcome<Credentials>.Fail(
                    new ConfigurationException("Client credentials have not been provisioned"));

            var secret = context.Configuration.ClientSecret;
            return Outcome<Credentials>.Success(new BasicAuthCredentials(identity!, secret!));
        }
    }
}