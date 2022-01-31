using System;

namespace TetraPak.XP
{
    /// <summary>
    ///   Implementors of this class can represent options for an individual
    ///   time limited repository.  
    /// </summary>
    public interface ITimeLimitedRepositoryOptions
    {
        /// <summary>
        ///   Gets a <see cref="TimeSpan"/> value that specifies an interval for how often 
        ///   a request for automatic purging of dead (expired) values should occur.
        ///   The value can be ignored by individual <see cref="ITimeLimitedRepositories"/> implementations
        ///   that might not support automatic purging.  
        /// </summary>
        TimeSpan PurgeInterval { get; }
        
        /// <summary>
        ///   Gets the life span for entries of the cache repository.
        /// </summary>
        TimeSpan LifeSpan { get; }
        
        /// <summary>
        ///   Gets a lifespan to be re-applied to a value as it is being read from the cache, extending its life span.
        /// </summary>
        TimeSpan ExtendedLifeSpan { get; }
        
        /// <summary>
        ///   Gets the maximum total lifespan allowed for any cached value
        ///   within a cache repository. Setting this to anything other than zero will limit the
        ///   total lifespan of cached values to unlimited lifespan due to dynamic lifespan extension
        ///   when reading/updating values (see <see cref="ExtendedLifeSpan"/>).
        /// </summary>
        TimeSpan MaxLifeSpan { get; }
        
        /// <summary>
        ///   Gets a value that reduces the <see cref="LifeSpan"/> (and <see cref="ExtendedLifeSpan"/> when applicable).
        ///   Use this value to create a "safe zone" for values that have themselves a limited lifetime
        ///   (outside of the repository, such as access tokens), and that might expire within this time span. 
        /// </summary>
        TimeSpan AdjustedLifeSpan { get; }
    }
}