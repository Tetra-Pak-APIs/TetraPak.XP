using System;
using System.Net;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

namespace TetraPak.XP
{
    partial class HttpServerException
    {
        /// <summary>
        ///   <para>
        ///   Produces a <see cref="HttpServerException"/> to indicate that the server encountered an unexpected condition
        ///   that prevented it from fulfilling the request.
        ///   </para>
        ///   <para>
        ///   This error response is a generic "catch-all" response. Usually, this indicates the server cannot
        ///   find a better 5xx error code to response. Sometimes, server administrators log error responses like
        ///   the 500 status code with more details about the request to prevent the error from happening
        ///   again in the future.
        ///   </para>
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
        public static HttpServerException InternalServerError(string? message = null, Exception? innerException = null)
        {
            return new HttpServerException(HttpStatusCode.InternalServerError, 
                message ?? "Internal ServerError", innerException);
        }
    }
}