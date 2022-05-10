using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Specifies options for a <see cref="Grant"/> request.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public class GrantOptions
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
        ///   A (unique) id used to identify the requesting actor.
        ///   This information is needed for caching purposed by some flows.  
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
        public bool IsCaching => (Flags & GrantFlags.Cached) == GrantFlags.Cached;

        public override string ToString()
        {
            return $"{base.ToString()} [{Flags}]" ;
        }

        /// <summary>
        ///   Sets an arbitrary value to be carried by the <see cref="GrantOptions"/>.
        /// </summary>
        /// <param name="key">
        ///   Identifies the data value. 
        /// </param>
        /// <param name="data">
        ///   The data value being added.
        /// </param>
        /// <seealso cref="GetDataAsync{T}"/>
        public void SetData(string key, object data) => _data[key] = data;

        /// <summary>
        ///   Sets a callback handler ("factory") for reading an arbitrary value
        ///   to be carried by the <see cref="GrantOptions"/>.
        /// </summary>
        /// <param name="key">
        ///   Identifies the data value. 
        /// </param>
        /// <param name="factory">
        ///   The data value factory added.
        /// </param>
        /// <seealso cref="GetDataAsync{T}"/>
        public void SetDataFactory<T>(string key, Func<T> factory) => _data[key] = factory;

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
        public async Task<T> GetDataAsync<T>(string key, T useDefault = default!)
        {
            if (!_data.TryGetValue(key, out var obj))
                return useDefault;

            return obj switch
            {
                T tValue => tValue,
                Func<T> func => func(),
                Func<Task<T>> funcAsync => await funcAsync(),
                _ => useDefault
            };
        }

        /// <summary>
        ///   Gets a value indicating whether the Refresh Grant flow is allowed
        ///   when a refresh token is available. 
        /// </summary>
        public bool IsRefreshAllowed => (Flags & GrantFlags.Refresh) == GrantFlags.Refresh;

        /// <summary>
        ///   Constructs and returns a default <see cref="GrantOptions"/> for use when requesting a grant.
        /// </summary>
        /// <param name="cancellationTokenSource">
        ///   (optional)<br/>
        ///   Allows cancelling the operation.
        /// </param>
        /// <param name="clientCredentials">
        ///   (optional; default=[see remarks])<br/>
        ///   Specifies client (a client id and, optionally, secret) credentials for the request.
        ///   Please see remarks for more details.
        /// </param>
        /// <returns>
        ///   A <see cref="GrantOptions"/> object.
        /// </returns>
        /// <remarks>
        ///   Please note that when no (<paramref name="clientCredentials"/>) are passed, and the grant type requires
        ///   that information, the service is expected to support some other means of obtaining them, such as
        ///   from a configuration source. 
        /// </remarks>
        /// <seealso cref="Grant"/>
        /// <seealso cref="GrantOptionsHelper.WithClientCredentials"/>
        public static GrantOptions Default(
            CancellationTokenSource? cancellationTokenSource = null,
            Credentials? clientCredentials = null) 
            => Silent(cancellationTokenSource, clientCredentials: clientCredentials);
        
        /// <summary>
        ///   Constructs and returns a <see cref="GrantOptions"/> object to force the grant request.
        /// </summary>
        /// <param name="cancellationTokenSource">
        ///   (optional)<br/>
        ///   Enables canceling the request.
        /// </param>
        /// <seealso cref="GrantFlags"/>
        /// <param name="actorId">
        ///   (optional)<br/>
        ///   A (unique) id used to identify the actor requesting authorization.
        ///   This information is needed for caching purposed by some grant flows.  
        /// </param>
        /// <param name="clientCredentials">
        ///   (optional; default=[see remarks])<br/>
        ///   Specifies client (a client id and, optionally, secret) credentials for the request.
        ///   Please see remarks for more details.
        /// </param>
        /// <remarks>
        ///   Please note that when no (<paramref name="clientCredentials"/>) are passed, and the grant type requires
        ///   that information, the service is expected to support some other means of obtaining them, such as
        ///   from a configuration source. 
        /// </remarks>
        /// <seealso cref="GrantFlags"/>
        /// <seealso cref="GrantOptionsHelper.WithClientCredentials"/>
        public static GrantOptions Forced(
            CancellationTokenSource? cancellationTokenSource = null,
            string? actorId = null,
            Credentials? clientCredentials = null)
        {
            return new GrantOptions
            {
                Flags = GrantFlags.Forced,
                CancellationTokenSource = cancellationTokenSource,
                ActorId = actorId,
                
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
        /// <param name="clientCredentials">
        ///   (optional; default=[see remarks])<br/>
        ///   Specifies client (a client id and, optionally, secret) credentials for the request.
        ///   Please see remarks for more details.
        /// </param>
        /// <remarks>
        ///   Please note that when no (<paramref name="clientCredentials"/>) are passed, and the grant type requires
        ///   that information, the service is expected to support some other means of obtaining them, such as
        ///   from a configuration source. 
        /// </remarks>
        /// <seealso cref="GrantFlags"/>
        /// <seealso cref="GrantOptionsHelper.WithClientCredentials"/>
        public static GrantOptions Forced(
            string? service, 
            CancellationTokenSource? cancellationTokenSource = null, 
            string? actorId = null,
            Credentials? clientCredentials = null)
        {
            return new GrantOptions
            {
                Service = service,
                Flags = GrantFlags.Forced,
                CancellationTokenSource = cancellationTokenSource,
                ActorId = actorId
            }.WithClientCredentials(clientCredentials);
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
        /// <param name="clientCredentials">
        ///   (optional; default=[see remarks])<br/>
        ///   Specifies client (a client id and, optionally, secret) credentials for the request.
        ///   Please see remarks for more details.
        /// </param>
        /// <remarks>
        ///   Please note that when no (<paramref name="clientCredentials"/>) are passed, and the grant type requires
        ///   that information, the service is expected to support some other means of obtaining them, such as
        ///   from a configuration source. 
        /// </remarks>
        /// <seealso cref="GrantFlags"/>
        /// <seealso cref="GrantOptionsHelper.WithClientCredentials"/>
        public static GrantOptions Silent(
            CancellationTokenSource? cancellationTokenSource = null, 
            string? actorId = null,
            Credentials? clientCredentials = null) 
            =>
            Silent(null, cancellationTokenSource, actorId, clientCredentials);
        
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
        /// <param name="clientCredentials">
        ///   (optional; default=[see remarks])<br/>
        ///   Specifies client (a client id and, optionally, secret) credentials for the request.
        ///   Please see remarks for more details.
        /// </param>
        /// <remarks>
        ///   Please note that when no (<paramref name="clientCredentials"/>) are passed, and the grant type requires
        ///   that information, the service is expected to support some other means of obtaining them, such as
        ///   from a configuration source. 
        /// </remarks>
        /// <seealso cref="GrantFlags"/>
        /// <seealso cref="GrantOptionsHelper.WithClientCredentials"/>
        public static GrantOptions Silent(
            string? service,
            CancellationTokenSource? cancellationTokenSource = null,
            string? actorId = null,
            Credentials? clientCredentials = null)
        {
            return new GrantOptions
            {
                Service = service,
                Flags = GrantFlags.Silent,
                ActorId = actorId,
                CancellationTokenSource = cancellationTokenSource
            }.WithClientCredentials(clientCredentials);
        }
    }
}