using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Xamarin;
using Xamarin.Essentials;

[assembly:XpService(typeof(XamarinFileSystem))]

namespace TetraPak.XP.Xamarin
{
    public class XamarinFileSystem : IFileSystem
    {
        public string GetCacheDirectory() => FileSystem.CacheDirectory;
    }
}