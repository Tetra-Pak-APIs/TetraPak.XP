using System.Collections.Generic;
using System.Threading;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Specifies options for a <see cref="Grant"/> request.
    /// </summary>
    public sealed class GrantOptions
    {
        readonly Dictionary<string, object> _data = new();

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
        ///   Sets an arbitrary value to be carried by the <see cref="GrantOptions"/>.
        /// </summary>
        /// <param name="key">
        ///   Identifies the data value. 
        /// </param>
        /// <param name="data">
        ///   The data value being added.
        /// </param>
        /// <seealso cref="GetData{T}"/>
        public void SetData(string key, object data) => _data[key] = data;
        
        /// <summary>
        ///   Gets an arbitrary value.
        /// </summary>
        /// <param name="key">
        ///   Identifies the data value. 
        /// </param>
        /// <param name="useDefault">
        ///   (optional)<br/>
        ///   A value to be returned if the requested data value isn't carried (or isn't of type <typeparamref name="T"/>). 
        /// </param>
        /// <typeparam name="T">
        ///   The type of data value being requested.
        /// </typeparam>
        /// <returns>
        ///   The requested data value (or <see cref="useDefault"/> if it isn't carried).
        /// </returns>
        /// <seealso cref="SetData"/>
        public T GetData<T>(string key, T useDefault = default!)
        {
            return _data.TryGetValue(key, out var obj) && obj is T tValue 
                ? tValue 
                : useDefault;
        }

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
            Silent(null, actorId, cancellationTokenSource);
        
        /// <summary>
        ///   Constructs and returns a <see cref="GrantOptions"/> object for a silent grant request.
        /// </summary>
        /// <param name="service">
        ///   (optional)<br/>
        ///   Specifies a service to be consumed. 
        /// </param>
        /// <param name="actorId">
        ///   (optional)<br/>
        ///   A (unique) id used to identify the actor requesting authorization. 
        ///   This information is needed for caching purposed by some grant flows.  
        /// </param>
        /// <param name="cancellationTokenSource">
        ///   (optional)<br/>
        ///   Enables canceling the request.
        /// </param>
        /// <seealso cref="GrantFlags"/>
        public static GrantOptions Silent(
            string? service,
            string? actorId = null,
            CancellationTokenSource? cancellationTokenSource = null)
        {
            return new GrantOptions
            {
                Service = service,
                Flags = GrantFlags.Silent,
                ActorId = actorId,
                CancellationTokenSource = cancellationTokenSource
            };
        }
    }
}