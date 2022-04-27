namespace TetraPak.XP.Web.Abstractions
{
    /// <summary>
    ///   Used by a <see cref="LoopbackFilter"/> to control how to handle loopback requests to a
    ///   <see cref="ILoopbackBrowser"/>. See <see cref="ILoopbackBrowser.GetLoopbackAsync"/> for more info
    ///   on how to use value it used.
    /// </summary>
    public enum LoopbackFilterOutcome
    {
        /// <summary>
        ///   The loopback request is accepted.
        /// </summary>
        Accept,
        
        /// <summary>
        ///   The loopback request should be ignored and listener should continue listening for more requests.
        /// </summary>
        Ignore,
        
        /// <summary>
        ///   The loopback listener should reject the request and stop listening for more requests. 
        /// </summary>
        RejectAndFail
    }
}