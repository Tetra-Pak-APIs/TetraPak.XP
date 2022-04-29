using System.IO;

namespace TetraPak.XP.Desktop
{
    class DesktopFileSystem : IFileSystem
    {
        readonly DirectoryInfo _cacheDirectory;

        public DirectoryInfo GetCacheDirectory() => _cacheDirectory;

        public DesktopFileSystem(string cachePath = "./.cache")
        {
            _cacheDirectory = new DirectoryInfo(cachePath);
        }
    }
}