using System;
using TetraPak.XP.DynamicEntities;

namespace TetraPak.XP.Caching
{
    public class RepositoryPath : DynamicPath
    {
        public new const string DefaultSeparator = "://";

        public string Repository => Items[0];

        public string Key => Items[1];

        public static implicit operator RepositoryPath(string stringValue)
        {
            return new RepositoryPath(stringValue);
        }

        public RepositoryPath(string stringValue) : base(stringValue)
        {
        }

        public RepositoryPath(string repository, string key) 
        : base($"{repository}{DefaultSeparator}{key}")
        {
        }
    }
}