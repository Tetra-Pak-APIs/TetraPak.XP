using TetraPak.XP.Caching.Abstractions;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   Classes implementing this interface can be registered as a token cache service, relying on
    ///   encryption to protect all cached values. 
    /// </summary>
    public interface ITokenCache : ISecureCache
    {
    }
}