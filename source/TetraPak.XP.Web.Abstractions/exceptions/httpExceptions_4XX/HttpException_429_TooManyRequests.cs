using System;
using System.Globalization;
using Microsoft.Net.Http.Headers;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP
{

    partial class HttpServerException
    {
        /// <summary>
        ///   <para>
        ///   Produces a <see cref="HttpServerException"/> to indicate the user has sent too many requests in a given amount
        ///   of time ("rate limiting").
        ///   </para>
        ///   <para>
        ///   A <c>Retry-After</c> header might be included to this response indicating how long to wait before
        ///   making a new request.
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
        /// <seealso cref="TooManyRequests(DateTime,string?,Exception?)"/>
        public static HttpServerException TooManyRequests(
            TimeSpan? retryAfter,
            string? message = null,
            Exception? innerException = null)
        {
            var response = makeResponseMessage(HttpStatusCode.TooManyRequests);
            if (retryAfter is { })
            {
                response.Headers.Add(
                    HeaderNames.RetryAfter,
                    Math.Truncate(retryAfter.Value.TotalSeconds).ToString(CultureInfo.InvariantCulture));
            }

            return new HttpServerException(response, message, innerException);
        }

        /// <summary>
        ///   <para>
        ///   Produces a <see cref="HttpServerException"/> to indicate the user has sent too many requests in a given amount
        ///   of time ("rate limiting").
        ///   </para>
        ///   <para>
        ///   A <c>Retry-After</c> header might be included to this response indicating how long to wait before
        ///   making a new request.
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
        /// <seealso cref="TooManyRequests(Nullable{System.TimeSpan},string?,Exception?)"/>
        public static HttpServerException TooManyRequests(
            DateTime retryAfter,
            string? message = null,
            Exception? innerException = null)
        {
            var response = makeResponseMessage(HttpStatusCode.TooManyRequests);
            response.Headers.Add(
                HeaderNames.RetryAfter,
                retryAfter.ToUniversalTime().ToString("R"));
            return new HttpServerException(response, message, innerException);
        }
    }
}