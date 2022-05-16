using System;

namespace TetraPak.XP
{
    
    /// <summary>
    ///   Used to specify how to adjust <see cref="Version"/> values.
    /// </summary>
    /// <seealso cref="VersionHelper.Adjust"/>
    public enum VersioningPolicy
    {
        /// <summary>
        ///   Items with a higher version will not be affected when "harmonizing" to a lower version.
        /// </summary>
        Soft,
        
        /// <summary>
        ///   Items with a higher version will be lowered when "harmonizing" to a lower version.
        /// </summary>
        Hard
    }
}