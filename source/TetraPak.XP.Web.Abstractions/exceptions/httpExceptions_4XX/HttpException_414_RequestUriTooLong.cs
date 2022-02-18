using System;
using System.Net;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

namespace TetraPak.XP
{
    partial class HttpServerException
    {
        /// <summary>
        ///   <para>
        ///   Produces a <see cref="HttpServerException"/> to indicate that the URI requested by the client is longer than
        ///   the server is willing to interpret.
        ///   </para>
        ///   <para>
        ///   There are a few rare conditions when this might occur:
        ///   </para>
        ///   <list type="bullet">
        ///     <item>
        ///     <description>
        ///     when a client has improperly converted a POST request to a GET request with long query information,
        ///     </description>
        ///     </item>
        ///     <item>
        ///     <description>
        ///     when the client has descended into a loop of redirection (for example, a redirected URI prefix that points to a suffix of itself),
        ///     </description>
        ///     </item>
        ///     <item>
        ///     <description>
        ///     or when the server is under attack by a client attempting to exploit potential security holes.
        ///     </description>
        ///     </item>
        ///   </list>
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
        public static HttpServerException RequestUriTooLong(
            string? message = null, 
            Exception? innerException = null)
        {
            return new HttpServerException(HttpStatusCode.RequestUriTooLong, 
                message ?? "Request Uri too Long", innerException);
        }
    }
}