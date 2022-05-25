using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;

[assembly:XpService(typeof(IPlatformService), typeof(PlatformService))]

namespace TetraPak.XP.Desktop
{
    public sealed class PlatformService : IPlatformService
    {
        public RuntimePlatform RuntimePlatform { get; }

        public Task<Outcome> TryCloseTopWindowAsync(bool isModalWindow, bool animated = true)
        {
            // todo Implement IPlatformService.TryCloseTopWindowAsync for desktop
            return  Task.FromResult(Outcome.Fail("Not (yet) implemented for desktop"));
        }

        static RuntimePlatform resolveRuntimePlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return RuntimePlatform.Windows;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return RuntimePlatform.MacOS;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return RuntimePlatform.Linux;

            throw new Exception("Cannot resolve runtime platform");
        }

        public PlatformService()
        {
            RuntimePlatform = resolveRuntimePlatform();
        }
    }
}