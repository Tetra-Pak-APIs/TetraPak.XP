using System;
using System.Diagnostics;
using TetraPak.XP.Caching.Abstractions;

namespace TetraPak.XP.Caching
{
    [DebuggerDisplay("{ToString()}")]
    public class SimpleCacheEntry : ITimeLimitedRepositoryEntry
    {
        public ITimeLimitedRepositories Repositories { get; }
        
        public DateTime SpawnTimeUtc { get; set; }

        DateTime InitialSpawnTimeUtc { get; set; }

        internal TimeSpan? CustomLifeSpan { get; private set; }

        internal TimeSpan? CustomMaxLifeSpan { get; private set; }
        
        object _value;
        
        readonly RepositoryPath _path;

        internal IITimeLimitedRepositoriesDelegate? SourceDelegate { get; set; }

        protected IITimeLimitedRepositoriesDelegate? GetSourceDelegate() => SourceDelegate;
        
        public string Path { get; set; }
        
        public Type GetValueType() => _value.GetType();

        public ITimeLimitedRepositoryEntry Clone() => new SimpleCacheEntry(Repositories, _path, _value, SpawnTimeUtc);

        public virtual T GetValue<T>() => (T)_value;

        public TimeSpan GetRemainingTime(DateTime? from = null)
        {
            from ??= DateTime.UtcNow;
            var maxLifeSpan = getMaxLifeSpan();
            var spawnTime = maxLifeSpan == TimeSpan.Zero
                ? SpawnTimeUtc
                : InitialSpawnTimeUtc;
            
            var lifeSpan = getLifeSpan();
            if (maxLifeSpan != TimeSpan.Zero && lifeSpan > maxLifeSpan)
            {
                lifeSpan = maxLifeSpan;
            }

            var adjustedLifeSpan = getAdjustedLifeSpan();
            if (adjustedLifeSpan != TimeSpan.Zero)
            {
                lifeSpan = lifeSpan.Add(adjustedLifeSpan);
            }
            var expires = spawnTime.Add(lifeSpan);
            return from < expires 
                ? expires.Subtract(from.Value) 
                : TimeSpan.Zero;
        }

        public override string ToString() => Path;

        TimeSpan getLifeSpan() => CustomLifeSpan ?? Repositories.GetLifeSpan(_path.Repository);

        TimeSpan getMaxLifeSpan() => CustomMaxLifeSpan ?? Repositories.GetMaxLifeSpan(_path.Repository);

        TimeSpan getAdjustedLifeSpan() => Repositories.GetAdjustedLifeSpan(_path.Repository);

        public object GetValue() => _value;

        public void UpdateValue(object value, DateTime? spawnTimeUtc = null, TimeSpan? customLifeSpan = null)
        {
            spawnTimeUtc ??= DateTime.UtcNow;
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            
            if (Repositories.IsTypeStrict && !value.GetType().IsInstanceOfType(_value))
                throw new InvalidCastException();
            
            _value = value;
            if (spawnTimeUtc.Value.Kind != DateTimeKind.Utc)
            {
                spawnTimeUtc = spawnTimeUtc.Value.ToUniversalTime();
            }
            SpawnTimeUtc = spawnTimeUtc.Value;

            if (customLifeSpan.HasValue)
            {
                CustomLifeSpan = customLifeSpan;
            }

            var extendedLifSpan = Repositories.GetExtendedLifeSpan(Path);
            if (extendedLifSpan != TimeSpan.Zero)
            {
                ExtendLifeSpan(spawnTimeUtc);
            }
        }

        public void ExtendLifeSpan(DateTime? spawnTimeUtc = null)
        {
            SpawnTimeUtc = spawnTimeUtc ?? DateTime.UtcNow;
        }

        public SimpleCacheEntry(
            ITimeLimitedRepositories repositories,
            string path,
            object value,
            DateTime? spawnTimeUtc = null)
        {
            _path = path;
            Path = path;
            _value = value ?? throw new ArgumentNullException(nameof(value));
            Repositories = repositories;
            InitialSpawnTimeUtc = SpawnTimeUtc = spawnTimeUtc ?? DateTime.UtcNow;
        }

        public SimpleCacheEntry(
            ITimeLimitedRepositories repositories,
            string path,
            object value,
            DateTime spawnTimeUtc,
            TimeSpan? customLifeSpan = null,
            TimeSpan? customMaxLifeSpan = null)
        {
            _path = path;
            Path = path;
            _value = value ?? throw new ArgumentNullException(nameof(value));
            Repositories = repositories;
            CustomLifeSpan = customLifeSpan;
            CustomMaxLifeSpan = customMaxLifeSpan;
            InitialSpawnTimeUtc = SpawnTimeUtc = spawnTimeUtc;
        }
    }
}