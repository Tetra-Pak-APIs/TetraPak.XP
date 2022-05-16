namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    static class InternalStringExtensions
    {
        public static string RemoveTrailingSlash(this string self) => self.TrimPostfix("/");
    }
}