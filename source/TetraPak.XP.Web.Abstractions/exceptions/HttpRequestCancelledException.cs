using System;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP
{

    /// <summary>
    ///   Reflects a situation where a HTTP request/response roundtrip was cancelled.
    /// </summary>
    public sealed class HttpRequestCancelledException : Exception
    {
        /// <summary>
        ///   Initializes the <see cref="HttpRequestCancelledException"/>.
        /// </summary>
        /// <param name="message">
        ///   (optional)<br/>
        ///   Describes the exception.
        /// </param>
        public HttpRequestCancelledException(string? message = null)
            : base(message ?? "Request was aborted")
        {
        }
    }
}