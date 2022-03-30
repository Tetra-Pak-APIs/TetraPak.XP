using System;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP;

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
    public static HttpServerException NotAcceptable(
        string? message = null, 
        Exception? innerException = null)
    {
        return new HttpServerException(HttpStatusCode.NotAcceptable, 
            message ?? "Not Acceptable", innerException);
    }
}
