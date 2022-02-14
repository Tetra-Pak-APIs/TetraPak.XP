using TetraPak.XP.DependencyInjection;

namespace TetraPak.XP.Desktop
{
    public static class DesktopServicesHelper
    {
        /// <summary>
        ///   Just a convenient (fluent api) method to automatically register all services
        ///   of the "Desktop" package. 
        /// </summary>
        public static XpServicesBuilder Desktop(this XpPlatformServicesBuilder builder) => builder.Build();
    }
}