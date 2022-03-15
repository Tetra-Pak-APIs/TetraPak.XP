using System;

namespace TetraPak.XP.Auth.Abstractions
{
    [Flags]
    public enum GrantFlags
    {
        /// <summary>
        ///   No grant options are specified.
        ///   This is usually treated as a signal to fallback to default options.
        /// </summary>
        None = 0,
        
        /// <summary>
        ///   Grant request is permitted to use cached grant outcome when possible.
        /// </summary>
        Cached = 1,
        
        /// <summary>
        ///   Forces a new grant request (depending on the grant type, this might mean cached grant
        ///   is not used but a refresh grant might be invoked).
        /// </summary>
        Forced = 2,
        
        /// <summary>
        ///   Grant request is permitted to invoke a Refresh grant to produce a new grant, when applicable. 
        /// </summary>
        Refresh = 4,
        
        /// <summary>
        ///   Grant request can be 'silent' (use cached grant when possible and then fallback to
        ///   invoking the refresh grant if applicable
        ///   (this is the equivalent to <see cref="Cached"/> & <see cref="Refresh"/>).
        /// </summary>
        Silent = Cached | Refresh
    }
}