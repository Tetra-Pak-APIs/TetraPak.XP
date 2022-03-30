using System;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP;

partial class HttpServerException
{
    /// <summary>
    ///   <para>
    ///   Produces a <see cref="HttpServerException"/> to indicate that the server cannot serve the requested ranges. The
    ///   most likely reason is that the document doesn't contain such ranges, or that the Range header value,
    ///   though syntactically correct, doesn't make sense.
    ///   </para>
    ///   <para>
    ///   The <c>416</c> response message contains a <c>Content-Range</c> indicating an unsatisfied range
    ///   (that is a <c>'*'</c>) followed by a <c>'/'</c> and the current length of the resource.
    ///   E.g. <c>Content-Range: bytes */12777</c>
    ///   </para>
    ///   <para>
    ///   Faced with this error, browsers usually either abort the operation
    ///   (for example, a download will be considered as non-resumable) or ask for the whole document again.
    ///   </para>
    /// </summary>
    /// <param name="unsatisfiedRange">
    ///   The unsatisfied length.
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
    public static HttpServerException RequestedRangeNotSatisfiable(
        ulong unsatisfiedRange,
        string? message = null, 
        Exception? innerException = null)
    {
        var response = new HttpResponseMessage((System.Net.HttpStatusCode)HttpStatusCode.RequestedRangeNotSatisfiable);
        response.Headers.Add(HeaderNames.ContentRange, $"*/{unsatisfiedRange.ToString()}");
        return new HttpServerException(response, message ?? "Request Timeout", innerException);
    }
}
