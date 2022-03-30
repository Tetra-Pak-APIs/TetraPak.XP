using System;
using System.Globalization;
using Microsoft.Net.Http.Headers;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP;

partial class HttpServerException
{
    /// <summary>
    ///   <para>
    ///   Produces a <see cref="HttpServerException"/> to indicate that the server does not support the functionality
    ///   required to fulfill the request.
    ///   </para>
    ///   <para>
    ///   This status can also send a <c>Retry-After</c> header, telling the requester when to check back to see if the
    ///   functionality is supported by then.
    ///   </para>
    ///   <para>
    ///   <c>501</c> is the appropriate response when the server does not recognize the request method and is
    ///   incapable of supporting it for any resource. The only methods that servers are required to support
    ///   (and therefore that must not return <c>501</c>) are <c>GET</c> and <c>HEAD</c>.
    ///   </para>
    ///   If the server does recognize the method, but intentionally does not support it,
    ///   the appropriate response is <c>405 Method Not Allowed</c>.
    ///   <para>
    ///   </para>
    /// </summary>
    /// <param name="retryAfter">
    ///   (optional; pass <c>null</c> if not needed)<br/>
    ///   Specifies a timespan to be sent back with the <c>Retry-After</c> header.
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
    /// <seealso cref="ServiceUnavailable(DateTime,string?,Exception?)"/>
    public static HttpServerException ServiceUnavailable(
        TimeSpan? retryAfter,
        string? message = null, 
        Exception? innerException = null)
    {
        var response = makeResponseMessage(HttpStatusCode.ServiceUnavailable);
        if (retryAfter is { TotalSeconds: > 0 })
        {
            response.Headers.Add(HeaderNames.RetryAfter, Math.Truncate(retryAfter.Value.TotalSeconds).ToString(CultureInfo.InvariantCulture));
        }
        
        return new HttpServerException(response, message ?? "Service Unavailable", innerException);
    }
    
            /// <summary>
    ///   <para>
    ///   Produces a <see cref="HttpServerException"/> to indicate that the server does not support the functionality
    ///   required to fulfill the request.
    ///   </para>
    ///   <para>
    ///   This status can also send a <c>Retry-After</c> header, telling the requester when to check back to see if the
    ///   functionality is supported by then.
    ///   </para>
    ///   <para>
    ///   <c>501</c> is the appropriate response when the server does not recognize the request method and is
    ///   incapable of supporting it for any resource. The only methods that servers are required to support
    ///   (and therefore that must not return <c>501</c>) are <c>GET</c> and <c>HEAD</c>.
    ///   </para>
    ///   If the server does recognize the method, but intentionally does not support it,
    ///   the appropriate response is <c>405 Method Not Allowed</c>.
    ///   <para>
    ///   </para>
    /// </summary>
    /// <param name="retryAfter">
    ///   Specifies a date/time to be sent back with the <c>Retry-After</c> header.
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
    /// <seealso cref="NotImplemented(Nullable{TimeSpan},string?,Exception?)"/>
    public static HttpServerException ServiceUnavailable(
        DateTime retryAfter,
        string? message = null, 
        Exception? innerException = null)
    {
        var response = makeResponseMessage(HttpStatusCode.ServiceUnavailable);
        response.Headers.Add(HeaderNames.RetryAfter, retryAfter.ToString("R"));
        return new HttpServerException(response, message ?? "Service Unavailable", innerException);
    }
}