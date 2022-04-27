using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Mobile;
using Xamarin.Essentials;

[assembly:XpService(typeof(XamarinFileSystem))]

namespace TetraPak.XP.Mobile
{
    public class XamarinFileSystem : IFileSystem
    {
        public string GetCacheDirectory() => FileSystem.CacheDirectory;
    }
}