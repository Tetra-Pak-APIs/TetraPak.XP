namespace TetraPak.XP
{
    /// <summary>
    ///   Used to specify the targeted runtime platform.
    /// </summary>
    public enum RuntimePlatform
    {
        /// <summary>
        ///   Runtime platform is not specified or not resolved/known.
        /// </summary>
        Any, 

        /// <summary>
        ///  Indicates the iOS runtime platform.
        /// </summary>
        IOS,

        /// <summary>
        ///  Indicates the Android runtime platform.
        /// </summary>
        Android,

        /// <summary>
        ///  Indicates the Windows runtime platform.
        /// </summary>
        Windows,

        /// <summary>
        ///  Indicates Mac OS runtime platform.
        /// </summary>
        MacOS,
        
        /// <summary>
        ///  Indicates Linux runtime platform.
        /// </summary>
        Linux
    }
}