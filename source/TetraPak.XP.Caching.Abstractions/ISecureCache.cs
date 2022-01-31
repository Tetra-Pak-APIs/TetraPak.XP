namespace TetraPak.XP.Caching
{
    /// <summary>
    ///   Classes implementing this interface can be registered as a secure cache service, relying on
    ///   encryption to protect all cached values.
    /// </summary>
    public interface ISecureCache : ITimeLimitedRepositories
    {
    }
}