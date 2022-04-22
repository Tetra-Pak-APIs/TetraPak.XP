using System;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP
{

    partial class HttpServerException
    {
        /// <summary>
        ///   Produces a <see cref="HttpServerException"/> to indicate that the server understands the content type of the
        ///   request entity, and the syntax of the request entity is correct,
        ///   but it was unable to process the contained instructions. 
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
        public static HttpServerException UnprocessableEntity(string? message = null, Exception? innerException = null)
        {
            return new HttpServerException(HttpStatusCode.UnprocessableEntity,
                message ?? "Unprocessable Entity", innerException);
        }
    }
}