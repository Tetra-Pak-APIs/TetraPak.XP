using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP
{

    partial class HttpServerException
    {
        /// <summary>
        ///   Produces a <see cref="HttpServerException"/> to reflect a situation where
        ///   the request method is known by the server but is not supported by the target resource.
        /// </summary>
        /// <param name="allowedMethods">
        ///   A list of allowed HTTP methods.
        /// </param>
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
        public static HttpServerException MethodNotAllowed(
            IEnumerable<string> allowedMethods,
            string? message = null,
            Exception? innerException = null)
        {
            var response = new HttpResponseMessage((System.Net.HttpStatusCode)HttpStatusCode.MethodNotAllowed);
            response.Headers.Add(HeaderNames.Allow, allowedMethods);
            return new HttpServerException(response, message ?? "Method not allowed", innerException);
        }
    }
}