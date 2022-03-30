using System;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP;

partial class HttpServerException
{
    /// <summary>
    ///   Produces a <see cref="HttpServerException"/> to reflect a situation where resource(s) was not found.
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
    public static HttpServerException NotFound(string? message = null, Exception? innerException = null)
    {
        return new HttpServerException(HttpStatusCode.NotFound, 
            message ?? "Not Found", innerException);
    }
}
