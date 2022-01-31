namespace TetraPak.XP.Auth
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
        iOS,

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
        MacOS
    }
}