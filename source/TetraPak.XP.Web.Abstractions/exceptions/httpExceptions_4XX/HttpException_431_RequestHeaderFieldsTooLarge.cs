using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// <param name="tooLongHeaders">
        ///   (optional; pass <c>null</c> if not needed)<br/>
        ///   Specifies one or more headers causing the problem.
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
        public static HttpServerException RequestHeaderFieldsTooLarge(
            IEnumerable<string>? tooLongHeaders,
            string? message = null,
            Exception? innerException = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.IsNullOrWhiteSpace(message)
                ? "Request Header Fields Too Large"
                : message);

            var longHeaders = tooLongHeaders as string[] ?? tooLongHeaders?.ToArray();
            if (longHeaders?.Any() ?? false)
            {
                sb.Append("Headers: ");
                sb.Append(longHeaders.ConcatEnumerable());
            }

            message = sb.ToString();
            return new HttpServerException(HttpStatusCode.RequestHeaderFieldsTooLarge, message, innerException);
        }
    }
}