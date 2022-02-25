using System.Threading;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Specifies options for a <see cref="Grant"/> request.
    /// </summary>
    public class GrantOptions
    {
        /// <summary>
        ///   Specifies how to perform the grant request.
        /// </summary>
        public GrantFlags Flags { get; set; }

        /// <summary>
        ///   Specifies a service to be consumed.  
        /// </summary>
        internal string? Service { get; set; }

        /// <summary>
        ///   A (unique) id used to identify the actor requesting authorization.
        ///   This information is needed for caching purposed by some grant flows.  
        /// </summary>
        public string? ActorId { get; set; }

        /// <summary>
        ///   Enables canceling the request.
        /// </summary>
        public CancellationTokenSource? CancellationTokenSource { get; set; }

        /// <summary>
        ///   Specifies a scope for the requested grant. 
        /// </summary>
        public GrantScope? Scope { get; set; }

        /// <summary>
        ///   Gets a value indicating whether cached grants are permitted. 
        /// </summary>
        public bool IsCachingAllowed => (Flags & GrantFlags.Cached) == GrantFlags.Cached;

        /// <summary>
        ///   Gets a value indicating whether the Refresh Grant flow is allowed
        ///   when a refresh token is available. 
        /// </summary>
        public bool IsRefreshAllowed => (Flags & GrantFlags.Refresh) == GrantFlags.Refresh;

        /// <summary>
        ///   Constructs and returns a <see cref="GrantOptions"/> object to force the grant request.
        /// </summary>
        /// <param name="cancellationTokenSource">
        ///   (optional)<br/>
        ///   Enables canceling the request.
        /// </param>
        /// <param name="actorId">
        ///   (optional)<br/>
        ///   A (unique) id used to identify the actor requesting authorization.
        ///   This information is needed for caching purposed by some grant flows.  
        /// </param>
        /// <seealso cref="GrantFlags"/>
        public static GrantOptions Forced(
            CancellationTokenSource? cancellationTokenSource = null, 
            string? actorId = null)
        {
            return new GrantOptions
            {
                Flags = GrantFlags.Forced,
                CancellationTokenSource = cancellationTokenSource,
                ActorId = actorId
            };
        }
        
        /// <summary>
        ///   Constructs and returns a <see cref="GrantOptions"/> object to force the grant request.
        /// </summary>
        /// <param name="service">
        ///   (optional)<br/>
        ///   Specifies a service to be consumed. 
        /// </param>
        /// <param name="cancellationTokenSource">
        ///   (optional)<br/>
        ///   Enables canceling the request.
        /// </param>
        /// <param name="actorId">
        ///   (optional)<br/>
        ///   A (unique) id used to identify the actor requesting authorization. 
        ///   This information is needed for caching purposed by some grant flows.  
        /// </param>
        /// <seealso cref="GrantFlags"/>
        public static GrantOptions Forced(
            string? service, 
            CancellationTokenSource? cancellationTokenSource = null, 
            string? actorId = null)
        {
            return new GrantOptions
            {
                Service = service,
                Flags = GrantFlags.Forced,
                CancellationTokenSource = cancellationTokenSource,
                ActorId = actorId
            };
        }

        /// <summary>
        ///   Constructs and returns a <see cref="GrantOptions"/> object for a silent grant request.
        /// </summary>
        /// <param name="cancellationTokenSource">
        ///   (optional)<br/>
        ///   Enables canceling the request.
        /// </param>
        /// <param name="actorId">
        ///   (optional)<br/>
        ///   A (unique) id used to identify the actor requesting authorization. 
        ///   This information is needed for caching purposed by some grant flows.  
        /// </param>
        /// <seealso cref="GrantFlags"/>
        public static GrantOptions Silent(
            CancellationTokenSource? cancellationTokenSource = null, 
            string? actorId = null) 
            =>
            Silent(null, cancellationTokenSource);
        
        /// <summary>
        ///   Constructs and returns a <see cref="GrantOptions"/> object for a silent grant request.
        /// </summary>
        /// <param name="service">
        ///   (optional)<br/>
        ///   Specifies a service to be consumed. 
        /// </param>
        /// <param name="cancellationTokenSource">
        ///   (optional)<br/>
        ///   Enables canceling the request.
        /// </param>
        /// <param name="actorId">
        ///   (optional)<br/>
        ///   A (unique) id used to identify the actor requesting authorization. 
        ///   This information is needed for caching purposed by some grant flows.  
        /// </param>
        /// <seealso cref="GrantFlags"/>
        public static GrantOptions Silent(
            string? service,
            CancellationTokenSource? cancellationTokenSource = null, 
            string? actorId = null)
        {
            return new GrantOptions
            {
                Service = service,
                Flags = GrantFlags.Silent,
                CancellationTokenSource = cancellationTokenSource
            };
        }
    }

    public static class GrantOptionsHelper
    {
        /// <summary>
        ///   (fluent api)<br/>
        ///   Assigns the <see cref="GrantOptions.Flags"/> property and returns the <see cref="GrantOptions"/>.
        /// </summary>
        /// <param name="options">
        ///   The <see cref="GrantOptions"/> object to be used for the grant request.   
        /// </param>
        /// <param name="scope">
        ///   The scope to be requested with the grant.
        /// </param>
        public static GrantOptions WithScope(this GrantOptions options, GrantScope scope)
        {
            options.Scope = scope;
            return options;
        }
    }
}