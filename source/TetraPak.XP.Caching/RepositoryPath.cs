using TetraPak.XP.DynamicEntities;

namespace TetraPak.XP.Caching
{
    public class RepositoryPath : DynamicPath
    {
        public new const string DefaultSeparator = "://";

        public string Repository => Items![0];

        public string Key => Items![1];

        public RepositoryPath(string repository, string key) 
        : base($"{repository}{DefaultSeparator}{key}")
        {
        }
    }
}