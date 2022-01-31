namespace TetraPak.AspNet.Debugging
{
    /// <summary>
    ///   Used to reflect HTTP traffic direction.
    /// </summary>
    public enum HttpDirection
    {
        /// <summary>
        ///   The traffic direction is not known in the current context.
        /// </summary>
        /// <seealso cref="NotApplicable"/>
        Unknown,
        
        /// <summary>
        ///   The traffic direction is not applicable in the current context.
        /// </summary>
        /// <seealso cref="Unknown"/>
        NotApplicable = Unknown,

        /// <summary>
        ///   Represents incoming traffic.
        /// </summary>
        /// <seealso cref="Upstream"/>
        In,
        
        /// <summary>
        ///   Represents incoming traffic.
        /// </summary>
        /// <seealso cref="In"/>
        Upstream = In,
        
        /// <summary>
        ///   Represents outgoing traffic.
        /// </summary>
        /// <seealso cref="Downstream"/>
        Out,
        
        /// <summary>
        ///   Represents outgoing traffic.
        /// </summary>
        /// <seealso cref="Out"/>
        Downstream = Out,
        
        /// <summary>
        ///   Represents a response to an outgoing request.
        /// </summary>
        Response
    }
}