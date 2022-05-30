namespace TetraPak.XP.ApplicationInformation
{
    /// <summary>
    ///   Describes a code framework or idiom. 
    /// </summary>
    public enum ApplicationPlatform
    {
        /// <summary>
        ///   Specifies a ASP.NET app.
        /// </summary>
        AspNet,

        /// <summary>
        ///   Specifies a console app.
        /// </summary>
        Console,
        
        /// <summary>
        ///   Specifies a windows app of unknown platform.
        /// </summary>
        WindowsOther,

        /// <summary>
        ///   Specifies a windows service.
        /// </summary>
        WindowsService,

        /// <summary>
        ///   Specifies a windows WPF app.
        /// </summary>
        WPF,
        
        /// <summary>
        ///   Specifies a Universal Windows Presentation app.
        /// </summary>
        UWP,

        /// <summary>
        ///   Specifies a Xamarin app.
        /// </summary>
        Xamarin,

        /// <summary>
        ///   Specifies a MAUI app.
        /// </summary>
        MAUI
    }
}