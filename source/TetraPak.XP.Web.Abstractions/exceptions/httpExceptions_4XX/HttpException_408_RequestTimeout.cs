using System;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP
{

    partial class HttpServerException
    {
        /// <summary>
        ///   Produces a <see cref="HttpServerException"/> to reflect a situation where the server would like to shut down
        ///   this unused connection. It is sent on an idle connection by some servers,
        ///   even without any previous request by the client.
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
        public static HttpServerException RequestTimeout(
            string? message = null,
            Exception? innerException = null)
        {
            var response = new HttpResponseMessage((System.Net.HttpStatusCode)HttpStatusCode.RequestTimeout);
            response.Headers.Add(HeaderNames.Connection, "close");
            return new HttpServerException(response, message ?? "Request Timeout", innerException);
        }
    }
}