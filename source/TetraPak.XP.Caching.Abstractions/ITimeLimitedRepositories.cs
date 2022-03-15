using System;
using System.Threading;
using System.Threading.Tasks;

namespace TetraPak.XP.Caching.Abstractions
{
    /// <summary>
    ///   Implementors of this contract can support repositories for arbitrary values
    ///   with a limited lifespan, which can be useful for caching purposes.
    /// </summary>
    public interface ITimeLimitedRepositories
    {
        /// <summary>
        ///   Gets or sets the name of a default repository to be assumed when a repository is not specified.
        ///   This can be useful when caching is implemented using separate cache instances for different
        ///   repositories.
        /// </summary>
        public string? DefaultRepository { get; set; }

        /// <summary>
        ///   Gets or sets the name of a default key to be assumed when a key is not specified.
        ///   This can be useful when caching is implemented to support a single item in a single (or more)
        ///   repositories.
        /// </summary>
        public string? DefaultKey { get; set; }
        
        /// <summary>
        ///   Gets or sets a default lifespan for repository values. 
        /// </summary>
        TimeSpan DefaultLifeSpan { get; set; }
        
        /// <summary>
        ///   Gets a default extended lifespan for repository values. The extended lifespan
        ///   is applied to entities as they are read or updated.
        /// </summary>
        /// <remarks>
        ///   Set this value to zero ("0" or "0:0:0") to avoid extending entries' cache lifespan.
        /// </remarks>
        TimeSpan DefaultExtendedLifeSpan { get; }
        
        /// <summary>
        ///   Gets a default maximum lifespan for repository values.
        ///   The maximum lifespan can never be exceeded by dynamically extending cached entries' lifespan.  
        /// </summary>
        /// <seealso cref="DefaultLifeSpan"/>
        /// <seealso cref="DefaultExtendedLifeSpan"/>
        /// <seealso cref="DefaultAdjustedLifeSpan"/>
        TimeSpan DefaultMaxLifeSpan { get; }
        
        /// <summary>
        ///   Gets a default adjusted lifespan for repository values.
        /// </summary>
        /// <seealso cref="DefaultLifeSpan"/>
        /// <seealso cref="DefaultExtendedLifeSpan"/>
        /// <seealso cref="DefaultMaxLifeSpan"/>
        TimeSpan DefaultAdjustedLifeSpan { get; }
        
        /// <summary>
        ///   Gets or sets a value specifying whether value types should be validated when updated.
        ///   Attempts to update an existing value with a value that is type incompatible will
        ///   throw an <see cref="IdentityConflictException"/>. 
        /// </summary>
        bool IsTypeStrict { get; set; }
        
        /// <summary>
        ///   Gets the lifespan configured for a specified repository. 
        /// </summary>
        /// <param name="repository">
        ///   Identifies the repository.    
        /// </param>
        /// <returns>
        ///   A <see cref="TimeSpan"/> value.
        /// </returns>
        TimeSpan GetLifeSpan(string repository);

        /// <summary>
        ///   Gets the maximum allowed lifespan configured for a specified repository. 
        /// </summary>
        /// <param name="repository">
        ///   Identifies the repository.    
        /// </param>
        /// <returns>
        ///   A <see cref="TimeSpan"/> value.
        /// </returns>
        TimeSpan GetMaxLifeSpan(string repository);
        
        /// <summary>
        ///   Gets the extended lifespan configured for a specified repository.
        ///   The extended lifespan is applied every time a cached entity is read or updated.  
        /// </summary>
        /// <param name="repository">
        ///   Identifies the repository.    
        /// </param>
        /// <returns>
        ///   A <see cref="TimeSpan"/> value.
        /// </returns>
        TimeSpan GetExtendedLifeSpan(string repository);

        /// <summary>
        ///   Gets the reduced lifespan configured for a specified repository.
        ///   The reduced lifespan is applied every time a cached entity is read or updated.  
        /// </summary>
        /// <param name="repository">
        ///   Identifies the repository.    
        /// </param>
        /// <returns>
        ///   A <see cref="TimeSpan"/> value.
        /// </returns>
        TimeSpan GetAdjustedLifeSpan(string repository);

        /// <summary>
        ///   Looks up a value in a repository.
        /// </summary>
        /// <param name="key">
        ///     Identifies the requested value.
        /// </param>
        /// <param name="repository">
        ///   (optional; default=<see cref="DefaultRepository"/>)<br/>
        ///   Identifies the repository where the value should exist.
        /// </param>
        /// <param name="cancellationToken">
        ///  (optional)<br/>
        ///   Allows canceling the request.
        /// </param>
        /// (optional)<br/>
        ///   Allows canceling the request.
        /// <typeparam name="T">
        ///   The requested value <see cref="Type"/>.
        /// </typeparam>
        /// <returns>
        ///   An <see cref="Exception"/> to indicate success/failure and also carry the requested value
        ///   (or an <see cref="Outcome{T}"/> on failure).
        /// </returns>
        Task<Outcome<T>> ReadAsync<T>(string key, string repository, CancellationToken? cancellationToken = null);

        /// <summary>
        ///   Adds a new time limited value.
        /// </summary>
        /// <param name="value">
        ///   The value to be added.
        /// </param>
        /// <param name="key">
        ///   Value's unique identifier.
        /// </param>
        /// <param name="repository">
        ///   (optional; default=<see cref="DefaultRepository"/>)<br/>
        ///   Identifies the repository where the new value should exist.
        /// </param>
        /// <param name="customLifeSpan">
        ///   (optional; default=<see cref="DefaultLifeSpan"/>)<br/>
        ///   A custom lifespan for the value.
        /// </param>
        /// <param name="spawnTimeUtc">
        ///   (optional; default=<see cref="DateTime.UtcNow"/>)<br/>
        ///   Specifies the spawn time for the value.
        /// </param>
        /// <exception cref="IdentityConflictException">
        ///   A value with the same <paramref name="key"/> was already added.
        /// </exception>
        Task CreateAsync(object value,
            string key,
            string repository,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null);

        /// <summary>
        ///   Updates an existing time limited value.
        /// </summary>
        /// <param name="value">
        ///   The value to be added.
        /// </param>
        /// <param name="key">
        ///   Value's unique identifier.
        /// </param>
        /// <param name="repository">
        ///   (optional; default=<see cref="DefaultRepository"/>)<br/>
        ///   Identifies the repository where the value should exist.
        /// </param>
        /// <param name="customLifeSpan">
        ///   (optional; default=<see cref="DefaultLifeSpan"/>)<br/>
        ///   A custom lifespan for the value.
        /// </param>
        /// <param name="spawnTimeUtc">
        ///   (optional; default=<see cref="DateTime.UtcNow"/>)<br/>
        ///   Specifies the spawn time for the value.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   A value could not be identified from the specified <paramref name="key"/>.
        /// </exception>
        Task UpdateAsync(
            object value,
            string key,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null);

        /// <summary>
        ///   Adds a new value or updated an existing one.
        /// </summary>
        /// <param name="value">
        ///     The value to be added or updated.
        /// </param>
        /// <param name="key">
        ///     Value's unique identifier.
        /// </param>
        /// <param name="repository">
        ///   (optional; default=<see cref="DefaultRepository"/>)<br/>
        ///   Identifies the repository where the value should exist.
        /// </param>
        /// <param name="customLifeSpan">
        ///   (optional; default=<see cref="DefaultLifeSpan"/>)<br/>
        ///   A custom lifespan for the value.
        /// </param>
        /// <param name="spawnTimeUtc">
        ///   (optional; default=<see cref="DateTime.UtcNow"/>)<br/>
        ///   Specifies the spawn time for the value.
        /// </param>
        /// <exception cref="InvalidCastException">
        ///   A value with the same <paramref name="key"/> was already added but its value is incompatible with the new value.
        /// </exception>
        Task CreateOrUpdateAsync(
            object value,
            string key,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null);

        /// <summary>
        ///   Removes a time limited value.
        /// </summary>
        /// <param name="key">
        ///     Value's unique identifier.
        /// </param>
        /// <param name="repository">
        ///     Identifies the repository to remove the value from.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   The <paramref name="key"/> was not recognized.
        /// </exception>
        Task DeleteAsync(string key, string? repository = null);

        /// <summary>
        ///   Creates or configures a time limited repository.
        /// </summary>
        /// <param name="repository">
        ///   Identifies the repository to be configured.
        /// </param>
        /// <param name="options">
        ///   Specifies the repository configuration.
        /// </param>
        public Task ConfigureAsync(string repository, ITimeLimitedRepositoryOptions options);

        /// <summary>
        ///   Obtains the configuration for a specified repository.
        /// </summary>
        /// <param name="repository">
        ///   Identifies the repository.
        /// </param>
        /// <param name="useDefault">
        ///   Specifies whether to return default options when none could be found.
        /// </param>
        /// <returns>
        ///   A <see cref="ITimeLimitedRepositoryOptions"/> instance.
        /// </returns>
        Task<ITimeLimitedRepositoryOptions> GetRepositoryOptionsAsync(string repository, bool useDefault = true);

        /// <summary>
        ///   Adds one more more repository delegates, to provide customized caching mechanism.
        /// </summary>
        /// <param name="delegates">
        ///   A collection of <see cref="IITimeLimitedRepositoriesDelegate"/> objects.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///   A delegate instance was already added.
        /// </exception>
        void AddDelegates(params IITimeLimitedRepositoriesDelegate[] delegates);

        /// <summary>
        ///   Examines the repository and returns a value indicating whether one or more supported delegates
        ///   implements a specified type. 
        /// </summary>
        /// <typeparam name="T">
        ///   The type of delegate to look for.
        /// </typeparam>
        /// <returns>
        ///   <c>true</c> if at least one supported delegate implements the specified type.
        /// </returns>
        bool ContainsDelegate<T>() where T : IITimeLimitedRepositoriesDelegate;

        /// <summary>
        ///   Inserts a cache delegate just before another delegate of a specified type.
        /// </summary>
        /// <param name="delegate">
        ///   The delegate to be inserted.
        /// </param>
        /// <typeparam name="T">
        ///   The type of delegate to look for, that will succeed the inserted delegate.
        /// </typeparam>
        /// <returns>
        ///   The internal index of the newly inserted delegate.
        /// </returns>
        /// <seealso cref="InsertDelegateBefore(IITimeLimitedRepositoriesDelegate,Func{IITimeLimitedRepositoriesDelegate, bool})"/>
        int InsertDelegateBefore<T>(IITimeLimitedRepositoriesDelegate @delegate)
            where T : IITimeLimitedRepositoriesDelegate;

        /// <summary>
        ///   Inserts a cache delegate just before another delegate that matches a custom criteria.
        /// </summary>
        /// <param name="delegate">
        ///   The delegate to be inserted.
        /// </param>
        /// <param name="criteria">
        ///   A callback handler to examine all existing delegates and return the one to be succeeded
        ///   by the inserted delegate.
        /// </param>
        /// <returns>
        ///   The internal index of the newly inserted delegate.
        /// </returns>
        /// <seealso cref="InsertDelegateBefore{T}(IITimeLimitedRepositoriesDelegate)"/>
        int InsertDelegateBefore(IITimeLimitedRepositoriesDelegate @delegate, Func<IITimeLimitedRepositoriesDelegate, bool> criteria);
    }
}