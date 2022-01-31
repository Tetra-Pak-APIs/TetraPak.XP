using System;
using System.Threading.Tasks;

namespace TetraPak.XP
{
    public interface IITimeLimitedRepositoriesDelegate 
    {
        /// <summary>
        ///   Gets or sets an interval between automatic purging processes.
        /// </summary>
        /// <remarks>
        ///   By setting this value to anything lower than <see cref="TimeSpan.MaxValue"/> the
        ///   <see cref="SimpleCache"/> instance will automatically remove all entries regularly,
        ///   to avoid resources leaks.  
        /// </remarks>
        TimeSpan AutoPurgeInterval { get; set; }
        
        /// <summary>
        ///   Validates a time limited repository entry and, when successful, converts it into a
        ///   <see cref="CachedItem{TValue}"/>.
        /// </summary>
        /// <param name="entry">
        ///   The time limited repository entry to be validated and converted.
        /// </param>
        /// <typeparam name="T">
        ///   The expected item type.
        /// </typeparam>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="CachedItem{TValue}"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        Task<Outcome<CachedItem<T>>> GetValidItemAsync<T>(ITimeLimitedRepositoryEntry entry);

        Task<Outcome<ITimeLimitedRepositoryEntry>> ReadRawEntryAsync(DynamicPath path);
        
        Task<Outcome> UpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict);
        
        Task<Outcome> DeleteAsync(DynamicPath path, bool strict);
        
        Task<Outcome> CreateAsync(ITimeLimitedRepositoryEntry entry, bool strict);

        Task PurgeNowAsync();
        
        Task<Outcome> CreateOrUpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict);
    }
}