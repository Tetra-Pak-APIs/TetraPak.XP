using System;
using System.Collections.Generic;
using System.IO;

namespace TetraPak.AspNet.Debugging
{
    /// <summary>
    ///   An abstract representation of a HTTP request.
    /// </summary>
    public abstract class AbstractHttpMessage
    {
        /// <summary>
        ///   (optional)<br/>
        ///   A unique string value for tracking a request/response (mainly for diagnostics purposes).
        /// </summary>
        public string? MessageId { get; set; }
        
        /// <summary>
        ///   Gets or sets a request URI.
        /// </summary>
        public Uri? Uri { get; set; }
        
        /// <summary>
        ///   Gets or sets a request HTTP method.
        /// </summary>
#pragma warning disable CS8618
        public string Method { get; set; }

        /// <summary>
        ///   Gets or sets the headers collection.
        /// </summary>
        public IEnumerable<KeyValuePair<string,IEnumerable<string>>> Headers { get; set; }
#pragma warning restore CS8618

        /// <summary>
        ///   Gets or sets a content (a.k.a. body).
        /// </summary>
        public Stream? Content { get; set; }
    }
}