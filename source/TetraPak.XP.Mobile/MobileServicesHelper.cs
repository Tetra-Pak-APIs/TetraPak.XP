using TetraPak.XP.DependencyInjection;

namespace TetraPak.XP.Mobile
{
    public static class MobileServicesHelper
    {
        /// <summary>
        ///   Just a convenient (fluent api) method to automatically register all services
        ///   of the "Mobile" package. 
        /// </summary>
        public static XpServicesBuilder Mobile(this XpPlatformServicesBuilder builder) => builder.Build();
    }
}