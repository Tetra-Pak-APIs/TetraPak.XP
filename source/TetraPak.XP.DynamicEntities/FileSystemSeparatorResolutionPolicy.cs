namespace TetraPak.XP.DynamicEntities
{
    public enum FileSystemSeparatorResolutionPolicy
    {
        /// <summary>
        ///   The UNIX file system separator: '/' is preferred.
        /// </summary>
        Unix,
        
        /// <summary>
        ///   The Windows file system separator: '\' is preferred.
        /// </summary>
        Windows,
        
        /// <summary>
        ///   The file system separator that is mostly used is also preferred.
        /// </summary>
        Majority
    }
}