using System;
using System.Net;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

namespace TetraPak.XP
{
    partial class HttpServerException
    {
        /// <summary>
        ///   Produces a <see cref="HttpServerException"/> to indicate a request conflict with
        ///   current state of the target resource.
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
        public static HttpServerException Conflict(
            string? message = null, 
            Exception? innerException = null)
        {
            return new HttpServerException(HttpStatusCode.Conflict, 
                message ?? "Conflict", innerException);
        }
    }
}