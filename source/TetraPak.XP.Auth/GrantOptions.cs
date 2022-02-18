using System.Threading;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Auth
{
    public class GrantOptions
    {
        /// <summary>
        ///   Specifies how to perform the grant request.
        /// </summary>
        public GrantFlags Flags { get; set; }

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
        ///   Constructs and returns a <see cref="GrantOptions"/> object to force the grant request.
        /// </summary>
        /// <param name="cancellationTokenSource">
        ///   Enables canceling the request.
        /// </param>
        /// <seealso cref="GrantFlags"/>
        public static GrantOptions Forced(CancellationTokenSource cancellationTokenSource)
        {
            return new GrantOptions
            {
                Flags = GrantFlags.Forced,
                CancellationTokenSource = cancellationTokenSource
            };
        }
        
        /// <summary>
        ///   Constructs and returns a <see cref="GrantOptions"/> object for a silent grant request.
        /// </summary>
        /// <param name="cancellationTokenSource">
        ///   Enables canceling the request.
        /// </param>
        /// <seealso cref="GrantFlags"/>
        public static GrantOptions Silent(CancellationTokenSource cancellationTokenSource)
        {
            return new GrantOptions
            {
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