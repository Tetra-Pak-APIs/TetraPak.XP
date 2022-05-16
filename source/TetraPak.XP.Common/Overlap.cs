namespace TetraPak.XP
{
    /// <summary>
    ///   Used to specify how two sets of values overlap. 
    /// </summary>
    public enum Overlap
    {
        /// <summary>
        ///   Sets do not overlap.
        /// </summary>
        None,
        
        /// <summary>
        ///   Sets fully overlap (one set completely overlaps the other or they are equal).
        /// </summary>
        Full,
        
        /// <summary>
        ///   In a comparison operation the left set overlaps the start of the right set. 
        /// </summary>
        Start,
        
        /// <summary>
        ///   In a comparison operation the left set overlaps the end of the right set. 
        /// </summary>
        End
    }
}