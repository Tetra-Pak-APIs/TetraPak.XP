using System;
using System.Net;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

namespace TetraPak.XP
{
    partial class HttpServerException
    {
        /// <summary>
        ///   Produces a <see cref="HttpServerException"/> to reflect a situation where
        ///   the server cannot or is unwilling to produce a response matching the request's list of acceptable
        ///   values, as defined in the content negotiation headers (Accept, Accept-Encoding, Accept-Language).
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
        public static HttpServerException ProxyAuthenticationRequired(
            string? message = null, 
            Exception? innerException = null)
        {
            return new HttpServerException(HttpStatusCode.ProxyAuthenticationRequired, 
                message ?? "Proxy Authentication Required", innerException);
        }
    }
}