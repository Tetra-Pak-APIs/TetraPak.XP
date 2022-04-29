using System.IO;

namespace TetraPak.XP
{
    /// <summary>
    ///   Classes implementing this contract can be used to interact with the local file system.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        ///   Returns the absolute path to the application's cache directory (if any).
        /// </summary>
        public DirectoryInfo GetCacheDirectory();
    }
}