using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace TetraPak.XP.Web.Abstractions
{
    /// <summary>
    ///   An abstract representation of a HTTP request.
    /// </summary>
    public abstract class AbstractHttpMessage
    {
        Uri? _uri;
        IQueryCollection? _query;

        /// <summary>
        ///   (optional)<br/>
        ///   A unique string value for tracking a request/response (mainly for diagnostics purposes).
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        ///   Gets or sets a request URI.
        /// </summary>
        public Uri? Uri
        {
            get => _uri;
            set
            {
                _uri = value;
                invalidateQuery();
            }
        }

        /// <summary>
        ///   Gets the query value collection parsed from the request.
        /// </summary>
        public IQueryCollection Query => getQuery();

        IQueryCollection getQuery()
        {
            if (_query is { })
                return _query;

            if (_uri is null)
                return new QueryCollection();

            return _query = new QueryCollection(QueryHelpers.ParseQuery(_uri.Query));
        }

        void invalidateQuery() => _query = null;

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
        
        internal string? ContentAsString { get; set; }
    }
}