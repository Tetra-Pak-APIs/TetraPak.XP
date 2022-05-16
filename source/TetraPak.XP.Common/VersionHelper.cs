using System;

namespace TetraPak.XP
{
    /// <summary>
    ///   Provides convenient helper methods for working with <see cref="Version"/> values.
    /// </summary>
    public static class VersionHelper
    {
        /// <summary>
        ///   Returns an adjusted <see cref="Version"/> value based on the extended (<paramref name="self"/>)
        ///   value and a target version. 
        /// </summary>
        /// <param name="self">
        ///   The extended <see cref="Version"/> value.
        /// </param>
        /// <param name="targetVersion">
        ///   A target <see cref="Version"/> value.
        /// </param>
        /// <param name="policy">
        ///   Specifies how to adjust the extended <see cref="Version"/> value.
        /// </param>
        /// <returns>
        ///   <list type="table">
        ///     <listheader>
        ///       <term>Policy</term>
        ///       <description>Returns</description>
        ///     </listheader>
        ///     <item>
        ///       <term><see cref="VersioningPolicy.Soft"/></term>
        ///       <description>
        ///         The <paramref name="targetVersion"/> if it is lower than the extended (<paramref name="self"/>)
        ///         value; otherwise the extended value.  
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term><see cref="VersioningPolicy.Hard"/></term>
        ///       <description>
        ///         The extended (<paramref name="self"/>) value.
        ///       </description>
        ///     </item>
        ///   </list>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   The <paramref name="policy"/> value was not supported.
        /// </exception>
        /// <seealso cref="VersioningPolicy"/>
        public static Version Adjust(this Version self, Version? targetVersion, VersioningPolicy policy = VersioningPolicy.Soft)
        {
            if (self == targetVersion || targetVersion is null)
                return self;
            
            return policy switch
            {
                VersioningPolicy.Soft => targetVersion < self ? self : targetVersion,
                VersioningPolicy.Hard => self,
                _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null)
            };
        }
    }
}