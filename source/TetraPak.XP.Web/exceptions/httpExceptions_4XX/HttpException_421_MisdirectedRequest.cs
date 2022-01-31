using System;
using System.Net;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

namespace TetraPak.XP
{
    partial class HttpServerException
    {
        /// <summary>
        ///   Produces a <see cref="HttpServerException"/> to indicate that the server is not able to produce a response.
        ///   This can be sent by a server that is not configured to produce responses for the combination of scheme
        ///   and authority that are included in the request URI.
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
        public static HttpServerException MisdirectedRequest(string? message = null, Exception? innerException = null)
        {
            return new HttpServerException(HttpStatusCode.MisdirectedRequest, 
                message ?? "Misdirected Request", innerException);
        }
    }
}