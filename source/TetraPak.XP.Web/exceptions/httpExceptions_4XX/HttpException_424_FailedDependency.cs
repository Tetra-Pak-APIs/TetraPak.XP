using System;
using System.Net;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

namespace TetraPak.XP
{
    partial class HttpServerException
    {
        /// <summary>
        ///   Produces a <see cref="HttpServerException"/> to indicate that the request failed
        ///   due to failure of a previous request.
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
        public static HttpServerException FailedDependency(string? message = null, Exception? innerException = null)
        {
            return new HttpServerException(HttpStatusCode.FailedDependency, 
                message ?? "Failed Dependency", innerException);
        }
    }
}