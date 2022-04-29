using System.IO;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Mobile;
using Xamarin.Essentials;

[assembly:XpService(typeof(XamarinFileSystem))]

namespace TetraPak.XP.Mobile
{
    public class XamarinFileSystem : IFileSystem
    {
        readonly DirectoryInfo _cacheDirectory;

        public DirectoryInfo GetCacheDirectory() => _cacheDirectory;

        public XamarinFileSystem()
        {
            _cacheDirectory = new DirectoryInfo(FileSystem.CacheDirectory);
        }
    }
    
     
}