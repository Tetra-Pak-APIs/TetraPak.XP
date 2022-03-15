namespace TetraPak.XP.Auth.Abstractions
{
    public interface IAppCredentialsDelegate
    {
        Outcome<Credentials> GetAppCredentials(AuthContext context);
    }
}