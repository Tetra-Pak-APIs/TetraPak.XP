namespace TetraPak.AspNet.Debugging
{
    /// <summary>
    ///   Provides <see cref="string"/> constants representing various standardised request initiators.
    /// </summary>
    public static class RequestInitiators
    {
        /// <summary>
        ///   The initiator is an actor (human or autonomous system).
        /// </summary>
        public const string Actor = nameof(Actor);
        
        /// <summary>
        ///   The initiator is a human actor.
        /// </summary>
        public const string HumanActor = nameof(HumanActor);
        
        /// <summary>
        ///   The initiator is an autonomous system.
        /// </summary>
        public const string SystemActor = nameof(SystemActor);
    }
}