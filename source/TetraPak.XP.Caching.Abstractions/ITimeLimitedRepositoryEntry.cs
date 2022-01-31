using System;

namespace TetraPak.XP.Caching.Abstractions
{
    /// <summary>
    ///   Implementors of this class can represent individual time limited repository entries. 
    /// </summary>
    public interface ITimeLimitedRepositoryEntry
    {
        /// <summary>
        ///   Gets or sets a path used to identify a value of the time limited repository. 
        /// </summary>
        string Path { get; set;  }
        
        DateTime SpawnTimeUtc { get; }
        
        /// <summary>
        ///   Returns the amount of time left before the entry expires. 
        /// </summary>
        /// <param name="from">
        ///   (optional; default=<see cref="DateTime.UtcNow"/>)<br/>
        ///   A point in time to calculate the remaining time from.
        /// </param>
        /// <returns>
        ///   A <see cref="TimeSpan"/> value.
        /// </returns>
        TimeSpan GetRemainingTime(DateTime? from = null);
        
        Type GetValueType();
        
        object GetValue();

        void UpdateValue(object value, DateTime? spawnTimeUtc = null, TimeSpan? customLifeSpan = null);

        void ExtendLifeSpan(DateTime? spawnTimeUtc = null);
        
        ITimeLimitedRepositories Repositories { get; }
    }
    
    // ReSharper disable once InconsistentNaming
    public static class ITimeLimitedRepositoryEntryExtensions
    {
        /// <summary>
        ///   Just a parameter-less variant of the <see cref="IsLive(ITimeLimitedRepositoryEntry, out DateTime)"/>
        ///   method, for cleaner syntax :-).
        /// </summary>
        public static bool IsLive(this ITimeLimitedRepositoryEntry self) => self.IsLive(out _);
        
        /// <summary>
        ///   Returns a value specifying whether the <see cref="ITimeLimitedRepositoryEntry"/> is
        ///   active at the time of the call.
        /// </summary>
        /// <param name="self">
        ///   The (extended) entry.
        /// </param>
        /// <param name="expiresUtc">
        ///   Passes back the time when the entry expires.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the en entry is live at the time of the invocation.
        /// </returns>
        public static bool IsLive(this ITimeLimitedRepositoryEntry self, out DateTime expiresUtc)
        {
            expiresUtc = self.ExpiresUtc(out var remainingTime);
            return remainingTime != TimeSpan.Zero;
        }

        /// <summary>
        ///   Just a parameter-less variant of the <see cref="ExpiresUtc(ITimeLimitedRepositoryEntry, out TimeSpan)"/>
        ///   method, for cleaner syntax :-).
        /// </summary>
        public static DateTime ExpiresUtc(this ITimeLimitedRepositoryEntry self) => self.ExpiresUtc(out var _);

        /// <summary>
        ///   Returns the date/time when the extended entry expires.
        /// </summary>
        /// <param name="self">
        ///   The extended entry. 
        /// </param>
        /// <param name="remainingTime">
        ///   Passes back how much time remains before the entry expires.
        /// </param>
        /// <returns>
        ///   
        /// </returns>
        public static DateTime ExpiresUtc(this ITimeLimitedRepositoryEntry self, out TimeSpan remainingTime)
        {
            remainingTime = self.GetRemainingTime();
            return DateTime.UtcNow.Add(remainingTime);
        }
    }
}