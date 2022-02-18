using System;
using System.Net;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

namespace TetraPak.XP
{
    partial class HttpServerException
    {
        /// <summary>
        ///   Produces a <see cref="HttpServerException"/> to indicate hat access to the target resource has been denied.
        ///   This happens with conditional requests on methods other than <c>GET</c> or <c>HEAD</c> when the condition
        ///   defined by the <c>If-Unmodified-Since</c> or <c>If-None-Match headers</c> is not fulfilled.
        ///   In that case, the request, usually an upload or a modification of a resource,
        ///   cannot be made and this error response is sent back. 
        /// </summary>
        /// <param name="message">
        ///   (optional)<br/>
        ///   Describes the problem.
        /// </param>
        /// <param name="innerException">
        ///   (optional)<br/>
        ///   The exception that is the cause of the current exception.
        /// </param>
        /// <returns>
        ///   A <see cref="HttpServerException"/>.
        /// </returns>
        public static HttpServerException PreconditionFailed(
            string? message = null, 
            Exception? innerException = null)
        {
            return new HttpServerException(HttpStatusCode.PreconditionFailed, 
                message ?? "Precondition Failed", innerException);
        }
    }
}