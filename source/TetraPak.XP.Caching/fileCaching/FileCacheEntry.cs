using System;
using TetraPak.XP.Caching.Abstractions;

namespace TetraPak.XP.Caching
{
    [Serializable]
    class FileCacheEntry : ITimeLimitedRepositoryEntry
    {
        // public string CachePath { get; set; }
        
        public string FilePath { get; set; }

        public string Path { get; set;  }
        public DateTime SpawnTimeUtc { get; set; }
        public TimeSpan GetRemainingTime(DateTime? @from = null)
        {
            throw new NotImplementedException();
        }

        public Type GetValueType()
        {
            throw new NotImplementedException();
        }

        public object GetValue()
        {
            throw new NotImplementedException();
        }

        public void UpdateValue(object value, DateTime? spawnTimeUtc = null, TimeSpan? customLifeSpan = null)
        {
            throw new NotImplementedException();
        }

        public void ExtendLifeSpan(DateTime? spawnTimeUtc = null)
        {
            throw new NotImplementedException();
        }

        public ITimeLimitedRepositories Repositories { get; private set; }
        
        public ITimeLimitedRepositoryEntry Clone()
        {
            return new FileCacheEntry(Path, FilePath)
            {
                Repositories = Repositories,
                SpawnTimeUtc = SpawnTimeUtc
            };
        }

        public TimeSpan? CustomLifeSpan { get; set; }

        public TimeSpan? CustomMaxLifeSpan { get; set; }

        public DateTime? LastAccessedUtc { get; set; }

        public FileCacheEntry(string cachePath, string filePath)
        {
            Path = cachePath;
            FilePath = filePath;
        }
    }
}