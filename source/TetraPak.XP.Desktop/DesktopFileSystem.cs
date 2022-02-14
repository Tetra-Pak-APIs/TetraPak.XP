using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;

[assembly:XpService(typeof(DesktopFileSystem))]

namespace TetraPak.XP.Desktop
{
    public class DesktopFileSystem : IFileSystem
    {
        public string GetCacheDirectory()
        {
            throw new System.NotImplementedException();
        }
    }
}