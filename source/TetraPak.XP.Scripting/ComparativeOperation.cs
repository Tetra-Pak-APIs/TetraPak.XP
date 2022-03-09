namespace TetraPak.XP.Scripting
{
    /// <summary>
    ///   used to express a comparison operation.
    /// </summary>
    public enum ComparativeOperation
    {
        /// <summary>
        ///   No (recognized) comparison operation is expressed. 
        /// </summary>
        None,
        
        /// <summary>
        ///   Specifies the "is equal" operation.
        /// </summary>
        Equal,
        
        /// <summary>
        ///   Specifies the "is not equal" operation.
        /// </summary>
        NotEqual,
        
        /// <summary>
        ///   Specifies the "is less than" operation.
        /// </summary>
        LessThan,

        /// <summary>
        ///   Specifies the "is less than, or equal to" operation.
        /// </summary>
        LessThanOrEquals,
        
        /// <summary>
        ///   Specifies the "is greater than" operation.
        /// </summary>
        GreaterThan,

        /// <summary>
        ///   Specifies the "is greater than, or equal to" operation.
        /// </summary>
        GreaterThanOrEquals,

        /// <summary>
        ///   Specifies the "contains" operation.
        /// </summary>
        Contains,
        
        /// <summary>
        ///   Specifies the "contains, or equal to" operation.
        /// </summary>
        ContainsOrEqual,

        /// <summary>
        ///   Specifies the "not contains" operation.
        /// </summary>
        NotContains,
        
        /// <summary>
        ///   Specifies the "is contained in" operation.
        /// </summary>
        Contained,
        
        /// <summary>
        ///   Specifies the "is contained in" operation.
        /// </summary>
        NotContained,
        
        /// <summary>
        ///   Specifies the "is contained in, or equal to" operation.
        /// </summary>
        ContainedOrEqual,
    }
}