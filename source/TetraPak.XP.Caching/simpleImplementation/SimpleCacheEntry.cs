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

        readonly TimeSpan? _customMaxLifeSpan;
        
        object _value;
        
        readonly RepositoryPath _path;

        internal IITimeLimitedRepositoriesDelegate? SourceDelegate { get; set; }

        protected IITimeLimitedRepositoriesDelegate? GetSourceDelegate() => SourceDelegate;
        
        public string Path { get; set; }
        
        // public bool IsLive(out DateTime expireTimeUtc) obsolete
        // {
        //     var remainingLifeSpan = GetRemainingTime();
        //     expireTimeUtc = DateTime.UtcNow.Add(remainingLifeSpan);
        //     return remainingLifeSpan != TimeSpan.Zero;
        // }

        public Type GetValueType() => _value.GetType();

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

        TimeSpan getMaxLifeSpan() => _customMaxLifeSpan ?? Repositories.GetMaxLifeSpan(_path.Repository);

        // TimeSpan getExtendedLifeSpan() => Repositories.GetExtendedLifeSpan(_path.Repository); obsolete

        TimeSpan getAdjustedLifeSpan() => Repositories.GetAdjustedLifeSpan(_path.Repository);

        public object GetValue() => _value;
        // {
        //     if (validate && !IsLive(out _))
        //         return Outcome<T>.Fail(new Exception("Value has expired")); obsolete
        //
        //     var outcome = _value is T tValue
        //         ? Outcome<T>.Success(tValue) 
        //         : Outcome<T>.Fail(new InvalidCastException("Value is not of requested type"));
        //
        //     if (!outcome)
        //         return outcome;
        //     
        //     var extendedLifSpan = getExtendedLifeSpan();
        //     if (extendedLifSpan != TimeSpan.Zero)
        //     {
        //         extendLifeSpan();
        //     }
        //
        //     return outcome;
        // }

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
            _path = (RepositoryPath)path;
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
            _path = (RepositoryPath)path;
            Path = path;
            _value = value ?? throw new ArgumentNullException(nameof(value));
            Repositories = repositories;
            CustomLifeSpan = customLifeSpan;
            _customMaxLifeSpan = customMaxLifeSpan;
            InitialSpawnTimeUtc = SpawnTimeUtc = spawnTimeUtc;
        }
    }
}