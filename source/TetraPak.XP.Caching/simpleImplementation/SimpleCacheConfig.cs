using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Caching
{
    /// <summary>
    ///   A configuration section specifying caching strategies. 
    /// </summary>
    public class SimpleCacheConfig : ConfigurationSectionWrapper, IEnumerable<KeyValuePair<string,ITimeLimitedRepositoryOptions>>
    {
        readonly Task _loadTask;
        SimpleCache? _cache;
        readonly Dictionary<string, ITimeLimitedRepositoryOptions> _repositoryConfigs;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, ITimeLimitedRepositoryOptions>> GetEnumerator()
        {
            awaitLoadingConfiguration();
            return _repositoryConfigs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///   Gets the configuration for a cache repository.
        /// </summary>
        /// <param name="repository">
        ///   Identifies the cache repository.
        /// </param>
        /// <returns>
        ///   A <see cref="ITimeLimitedRepositoryOptions"/> object.
        /// </returns>
        public ITimeLimitedRepositoryOptions? GetRepositoryOptions(string repository)
        {
            awaitLoadingConfiguration();
            return _repositoryConfigs.TryGetValue(repository, out var config)
                ? config
                : null;
        }

        /// <summary>
        ///   Configures a cache repository from a set of options.
        /// </summary>
        /// <param name="repository">
        ///   Identifies the cache repository to be configured.
        /// </param>
        /// <param name="options">
        ///   The configuration options. 
        /// </param>
        public void Configure(string repository, ITimeLimitedRepositoryOptions options)
        {
            awaitLoadingConfiguration();
            if (_repositoryConfigs.TryGetValue(repository, out var existing))
            {
                ((SimpleTimeLimitedRepositoryOptions) existing).MergeFrom(options);
                return;
            }

            _repositoryConfigs.Add(repository, options);
        }

        /// <summary>
        ///   (fluent UI)<br/>
        ///   Sets the caching mechanism and returns <c>this</c> instance. 
        /// </summary>
        public SimpleCacheConfig WithCache(SimpleCache cache)
        {
            awaitLoadingConfiguration();
            _cache = cache;
            foreach (var options in _repositoryConfigs.Values)
            {
                ((SimpleTimeLimitedRepositoryOptions) options).SetCache(cache);
            }
            return this;
        }

        void awaitLoadingConfiguration() => _loadTask.Await();
        
        Task loadRepositoryConfigsAsync()
        {
            return Task.Run(async () => 
            {
                var childSections = await GetChildrenAsync();
                foreach (var childSection in childSections)
                {
                    var config = new SimpleTimeLimitedRepositoryOptions(_cache, Section, childSection.Key, Log);
                    _repositoryConfigs.Add(childSection.Key, config);
                }
            });
            return Task.CompletedTask;
        }

        /// <summary>
        ///   Initializes the <see cref="SimpleCacheConfig"/>.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="configuration"></param>
        /// <param name="log"></param>
        /// <param name="key"></param>
        public SimpleCacheConfig(
            SimpleCache? cache,
            IConfiguration configuration, 
            string key,
            ILog? log = null) 
        : base(configuration, ValidateAssigned(key), log)
        {
            // todo This class needs to rely on the root configuration (implemented per platform) rather than inheritance
            _cache = cache;
            _repositoryConfigs = new Dictionary<string, ITimeLimitedRepositoryOptions>();
            _loadTask = loadRepositoryConfigsAsync(); // loads configuration in background
        }
    }

    public static class RepositoryHelper
    {
        public static TimeSpan GetRepositoryPurgeInterval(this SimpleCacheConfig self, string repository, TimeSpan useDefault)
            => self.GetRepositoryOptions(repository)?.PurgeInterval ?? useDefault;
    }
}