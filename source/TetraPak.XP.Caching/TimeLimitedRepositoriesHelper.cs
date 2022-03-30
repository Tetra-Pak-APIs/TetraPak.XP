using System;
using System.Threading.Tasks;
using TetraPak.XP.Caching.Abstractions;

namespace TetraPak.XP.Caching
{
    public static class TimeLimitedRepositoriesHelper
    {
        public static RepositoryPath GetValidPath(this ITimeLimitedRepositories? self, string? key, string? repository)
        {
            if (self is null)
                throw new ArgumentNullException(nameof(self));
                
            var useKey = key 
                         ?? self.DefaultKey 
                         ?? throw new InvalidOperationException("A key must be specified");
            var useRepository = repository 
                                ?? self.DefaultRepository 
                                ?? throw new InvalidOperationException("A repository must be specified");
            return new RepositoryPath(useRepository, useKey);
        }
        
        #region .  Support use of path in repository CRUD ops .

        public static Task CreateAsync(
            this ITimeLimitedRepositories? self, 
            object value,
            RepositoryPath path, 
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null) 
            =>
            self is null 
                ? Task.CompletedTask 
                : self.CreateAsync(value, path.Key, path.Repository, customLifeSpan, spawnTimeUtc);

        public static Task<Outcome<T>> ReadAsync<T>(this ITimeLimitedRepositories? self, RepositoryPath path) 
            =>
            self is { } 
                ? self.ReadAsync<T>(path.Key, path.Repository) 
                : failRepositoryUnavailable<T>();

        public static Task UpdateAsync(
            this ITimeLimitedRepositories? self, 
            object value,
            RepositoryPath path,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null) 
            =>
            self is { } 
                ? self.UpdateAsync(value, path.Key, path.Repository, customLifeSpan, spawnTimeUtc) 
                : Task.CompletedTask;

        public static Task CreateOrUpdateAsync(
            this ITimeLimitedRepositories? self, 
            object value,
            RepositoryPath path,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null) 
            =>
            self is { } 
                ? self.CreateOrUpdateAsync(value, path.Key, path.Repository, customLifeSpan, spawnTimeUtc) 
                : Task.CompletedTask;

        public static Task DeleteAsync(this ITimeLimitedRepositories? self, RepositoryPath path) 
            =>
            self is { } 
                ? self.DeleteAsync(path.Key, path.Repository) 
                : Task.CompletedTask;

        #endregion

        #region .  Attempt<CRUD> - allows invoking CRUS ops with unassigned cache .

        public static Task AttemptCreateAsync(
            this ITimeLimitedRepositories? self, 
            object value,
            string? key = null,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null) 
            =>
            self is { }
                ? self.CreateAsync(value, self.GetValidPath(key, repository), customLifeSpan, spawnTimeUtc)
                : Task.CompletedTask;

        public static Task<Outcome<T>> AttemptReadAsync<T>(
            this ITimeLimitedRepositories? self, 
            string? key = null, string? repository = null) 
            =>
            self is { }
                ? self.ReadAsync<T>(self.GetValidPath(key, repository))
                : Task.FromResult(Outcome<T>.Fail($"{nameof(ITimeLimitedRepositories)} is unassigned"));

        public static Task AttemptUpdateAsync(
            this ITimeLimitedRepositories? self, 
            object value,
            string? key= null,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null) 
            =>
            self is { }
                ? self.UpdateAsync(value, self.GetValidPath(key, repository), customLifeSpan, spawnTimeUtc)
                : Task.CompletedTask;

        public static Task AttemptCreateOrUpdateAsync(
            this ITimeLimitedRepositories? self, 
            object value,
            string? key = null,
            string? repository = null,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null) 
            =>
            self is { }
                ? self.CreateOrUpdateAsync(value, self.GetValidPath(key, repository), customLifeSpan, spawnTimeUtc)
                : Task.CompletedTask;

        public static Task AttemptDeleteAsync(
            this ITimeLimitedRepositories? self, 
            string? key = null,
            string? repository = null) 
            =>
            self is { }
                ? DeleteAsync(self, self.GetValidPath(key, repository))
                : Task.CompletedTask;

        #endregion

        #region .  Fluent api supporting DefaultKey and DefaultRepository  .
        
        public static ITimeLimitedRepositories WithDefaultKey(this ITimeLimitedRepositories self, string defaultKey)
        {
            self.DefaultKey = defaultKey;
            return self;
        }
        
        public static T WithDefaultKey<T>(this ITimeLimitedRepositories self, string defaultKey)
            where T : ITimeLimitedRepositories
        {
            self.DefaultKey = defaultKey;
            return (T)self;
        }

        /// <summary>
        ///   Creates a value without specifying its identity. The value is instead identified by the
        ///   repositories <see cref="ITimeLimitedRepositories.DefaultKey"/>
        ///   and <see cref="ITimeLimitedRepositories.DefaultRepository"/> identifiers.
        /// </summary>
        /// <seealso cref="ITimeLimitedRepositories.CreateAsync"/>
        /// <seealso cref="CreateAsync(ITimeLimitedRepositories?,object,RepositoryPath,TimeSpan?,DateTime?)"/>
        public static Task CreateAsync(
            this ITimeLimitedRepositories? self, 
            object value, 
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
        {
            if (self is null)
                return Task.CompletedTask;

            var key = self.DefaultKey;
            if (!key.IsAssigned())
                throw new InvalidOperationException($"Cannot add value to {self} without a specified key. Please consider assigning the {nameof(ITimeLimitedRepositories.DefaultKey)}");

            var repository = self.DefaultRepository;
            if (!repository.IsAssigned())
                throw new InvalidOperationException($"Cannot add value to {self} without a specified repository. Please consider assigning the {nameof(ITimeLimitedRepositories.DefaultRepository)}");
            
            return self.CreateAsync(value, key!, repository!, customLifeSpan, spawnTimeUtc);
        }
        
        /// <summary>
        ///   Reads a value without specifying its identity. The value is instead identified by the
        ///   repositories <see cref="ITimeLimitedRepositories.DefaultKey"/>
        ///   and <see cref="ITimeLimitedRepositories.DefaultRepository"/> identifiers.
        /// </summary>
        /// <seealso cref="ITimeLimitedRepositories.ReadAsync{T}"/>
        /// <seealso cref="ReadAsync{T}(ITimeLimitedRepositories?,RepositoryPath)"/>
        public static Task<Outcome<T>> ReadAsync<T>(this ITimeLimitedRepositories? self)
        {
            if (self is null)
                return failRepositoryUnavailable<T>();

            var key = self.DefaultKey;
            if (!key.IsAssigned())
                throw new InvalidOperationException($"Cannot read value from {self} without a specified key. Please consider assigning the {nameof(ITimeLimitedRepositories.DefaultKey)}");

            var repository = self.DefaultRepository;
            if (!repository.IsAssigned())
                throw new InvalidOperationException($"Cannot read value from {self} without a specified repository. Please consider assigning the {nameof(ITimeLimitedRepositories.DefaultRepository)}");

            return self.ReadAsync<T>(key!, repository!);
        }
        
        /// <summary>
        ///   Updates a value without specifying its identity. The value is instead identified by the
        ///   repositories <see cref="ITimeLimitedRepositories.DefaultKey"/>
        ///   and <see cref="ITimeLimitedRepositories.DefaultRepository"/> identifiers.
        /// </summary>
        /// <seealso cref="ITimeLimitedRepositories.UpdateAsync"/>
        /// <seealso cref="UpdateAsync(ITimeLimitedRepositories?,object,RepositoryPath,System.TimeSpan?,DateTime?)"/>
        public static Task UpdateAsync(
            this ITimeLimitedRepositories? self, 
            object value,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
        {
            if (self is null)
                return Task.CompletedTask;

            var key = self.DefaultKey;
            if (!key.IsAssigned())
                throw new InvalidOperationException($"Cannot update value in {self} without a specified key. Please consider assigning the {nameof(ITimeLimitedRepositories.DefaultKey)}");

            var repository = self.DefaultRepository;
            if (!repository.IsAssigned())
                throw new InvalidOperationException($"Cannot update value in {self} without a specified repository. Please consider assigning the {nameof(ITimeLimitedRepositories.DefaultRepository)}");

            return self.UpdateAsync(value, key!, repository!, customLifeSpan, spawnTimeUtc);
        }
        
        /// <summary>
        ///   Creates or updates a value without specifying its identity. The value is instead identified by the
        ///   repositories <see cref="ITimeLimitedRepositories.DefaultKey"/>
        ///   and <see cref="ITimeLimitedRepositories.DefaultRepository"/> identifiers.
        /// </summary>
        /// <seealso cref="ITimeLimitedRepositories.CreateOrUpdateAsync"/>
        /// <seealso cref="CreateOrUpdateAsync(ITimeLimitedRepositories?,object,RepositoryPath,TimeSpan?,DateTime?)"/>
        public static Task CreateOrUpdateAsync(
            this ITimeLimitedRepositories? self, 
            object value,
            TimeSpan? customLifeSpan = null,
            DateTime? spawnTimeUtc = null)
        {
            if (self is null)
                return Task.CompletedTask;

            var key = self.DefaultKey;
            if (!key.IsAssigned())
                throw new InvalidOperationException($"Cannot create or update value in {self} without a specified key. Please consider assigning the {nameof(ITimeLimitedRepositories.DefaultKey)}");

            var repository = self.DefaultRepository;
            if (!repository.IsAssigned())
                throw new InvalidOperationException($"Cannot create or update value in {self} without a specified repository. Please consider assigning the {nameof(ITimeLimitedRepositories.DefaultRepository)}");

            return self.CreateOrUpdateAsync(value, key!, repository!, customLifeSpan, spawnTimeUtc);
        }

        /// <summary>
        ///   Deletes a value without specifying its identity. The value is instead identified by the
        ///   repositories <see cref="ITimeLimitedRepositories.DefaultKey"/>
        ///   and <see cref="ITimeLimitedRepositories.DefaultRepository"/> identifiers.
        /// </summary>
        /// <seealso cref="ITimeLimitedRepositories.DeleteAsync"/>
        /// <seealso cref="DeleteAsync(ITimeLimitedRepositories?,RepositoryPath)"/>
        public static Task DeleteAsync(this ITimeLimitedRepositories? self)
        {
            if (self is null)
                return Task.CompletedTask;

            var key = self.DefaultKey;
            if (!key.IsAssigned())
                throw new InvalidOperationException($"Cannot delete value in {self} without a specified key. Please consider assigning the {nameof(ITimeLimitedRepositories.DefaultKey)}");

            var repository = self.DefaultRepository;
            if (!repository.IsAssigned())
                throw new InvalidOperationException($"Cannot delete value in {self} without a specified repository. Please consider assigning the {nameof(ITimeLimitedRepositories.DefaultRepository)}");

            return self.DeleteAsync(key!, repository!);
        }
        
        #endregion
        
        static Task<Outcome<T>> failRepositoryUnavailable<T>() 
            => Task.FromResult(Outcome<T>.Fail(new Exception( "Cache is unavailable")));

        public static ITimeLimitedRepositories WithDefaultRepository(this ITimeLimitedRepositories self, string repository)
        {
            self.DefaultRepository = repository;
            return self;
        }

        public static T WithDefaultRepository<T>(this ITimeLimitedRepositories self, string repository)
            where T : ITimeLimitedRepositories
        {
            self.DefaultRepository = repository;
            return (T)self;
        }

    }
}