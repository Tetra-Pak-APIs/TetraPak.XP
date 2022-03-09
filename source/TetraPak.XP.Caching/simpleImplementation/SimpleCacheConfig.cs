using System;
using System.Collections;
using System.Collections.Generic;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Configuration;

namespace TetraPak.XP.Caching
{
    /// <summary>
    ///   A configuration section specifying caching strategies. 
    /// </summary>
    public class SimpleCacheConfig : ConfigurationSectionWrapper, IEnumerable<KeyValuePair<string,ITimeLimitedRepositoryOptions>>
    {
        SimpleCache? _cache;
        readonly Dictionary<string, ITimeLimitedRepositoryOptions> _repositoryConfigs;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, ITimeLimitedRepositoryOptions>> GetEnumerator()
        {
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
            _cache = cache;
            foreach (var options in _repositoryConfigs.Values)
            {
                ((SimpleTimeLimitedRepositoryOptions) options).SetCache(cache);
            }
            return this;
        }

        // void awaitLoadingConfiguration() => _loadTask.Await();
        
        void loadRepositoryConfigsAsync()
        {
            var childSections = this.GetSubSections();
            foreach (var childSection in childSections)
            {
                var args = CreateSectionWrapperArgs(childSection, this);
                var config = new SimpleTimeLimitedRepositoryOptions(null, args);
                _repositoryConfigs.Add(childSection.Key, config);
            }
        }

        /// <summary>
        ///   Initializes the <see cref="SimpleCacheConfig"/>.
        /// </summary>
        public SimpleCacheConfig(SimpleCache? cache, ConfigurationSectionDecoratorArgs args)
        : base(args)
        {
            // todo This class needs to rely on the root configuration (implemented per platform) rather than inheritance
            _cache = cache;
            _repositoryConfigs = new Dictionary<string, ITimeLimitedRepositoryOptions>();
        }
    }

    public static class RepositoryHelper
    {
        public static TimeSpan GetRepositoryPurgeInterval(this SimpleCacheConfig self, string repository, TimeSpan useDefault)
            => self.GetRepositoryOptions(repository)?.PurgeInterval ?? useDefault;
    }
}