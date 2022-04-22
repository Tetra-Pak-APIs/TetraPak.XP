using System;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP
{

    partial class HttpServerException
    {
        /// <summary>
        ///   <para>
        ///   Produces a <see cref="HttpServerException"/> to indicate that the server refuses to accept the request because
        ///   the payload format is in an unsupported format.
        ///   </para>
        ///   <para>
        ///   The format problem might be due to the request's indicated <c>Content-Type</c> or <c>Content-Encoding</c>,
        ///   or as a result of inspecting the data directly. 
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
        public static HttpServerException UnsupportedMediaType(
            string? message = null,
            Exception? innerException = null)
        {
            return new HttpServerException(HttpStatusCode.UnsupportedMediaType,
                message ?? "Unsupported Media Type", innerException);
        }
    }
}