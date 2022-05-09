using System.Threading.Tasks;

namespace TetraPak.XP.OAuth2
{
    public interface IGrantService
    {
        /// <summary>
        ///   Gets a value that indicates whether the service can be cancelled.
        /// </summary>
        bool CanBeCanceled { get; }
        
        /// <summary>
        ///   Cancels the grant request.
        /// </summary>
        /// <returns>
        ///   An <see cref="Outcome"/> to indicate the canceled outcome.
        /// </returns>
        Task<bool> CancelAsync();
        
        Task ClearCachedGrantsAsync();

        Task ClearCachedRefreshTokensAsync();
    }
}