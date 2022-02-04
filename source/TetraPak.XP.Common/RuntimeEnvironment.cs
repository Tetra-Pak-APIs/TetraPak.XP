namespace TetraPak.XP
{
    /// <summary>
    ///   Specifies the targeted runtime environment.
    /// </summary>
    public enum RuntimeEnvironment
    {
        /// <summary>
        ///   Targets a production environment.
        /// </summary>
        Production,
        
        /// <summary>
        ///   Targets a migration environment.
        /// </summary>
        Migration,
        
        /// <summary>
        ///   Targets a test environment.
        /// </summary>
        Test,

        /// <summary>
        ///   Targets a development environment.
        /// </summary>
        Development,
    }
}