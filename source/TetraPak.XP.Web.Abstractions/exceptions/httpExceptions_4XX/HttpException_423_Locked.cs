using System;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP;

partial class HttpServerException
{
    /// <summary>
    ///   Produces a <see cref="HttpServerException"/> to indicate that the resource that is being accessed is locked.
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
    public static HttpServerException Locked(string? message = null, Exception? innerException = null)
    {
        return new HttpServerException(HttpStatusCode.Locked, 
            message ?? "Locked", innerException);
    }
}