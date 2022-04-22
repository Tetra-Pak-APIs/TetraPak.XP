using System;
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
        /// <param name="authorityUrl">
        ///   A URL to an authority to be referenced.
        /// </param>
        /// <param name="isBlockedBy">
        ///   Specifies whether the authority is blocking the request. Pass <c>true</c> if the "<c>ref="blocked-by"</c>"
        ///   flag is to be added to the <paramref name="authorityUrl"/>. 
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
        public static HttpServerException UnavailableForLegalReasons(
            string authorityUrl,
            bool isBlockedBy,
            string? message = null,
            Exception? innerException = null)
        {
            var response = makeResponseMessage(HttpStatusCode.UnavailableForLegalReasons);
            var link = new StringBuilder("<");
            link.Append(authorityUrl);
            if (isBlockedBy)
            {
                link.Append("; rel=\"blocked-by\"");
            }

            response.Headers.Add("Link", link.ToString());
            return new HttpServerException(response, message, innerException);
        }
    }
}