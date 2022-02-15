using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace TetraPak.XP.Web.Http.Debugging
{
    /// <summary>
    ///   Convenient methods for working with <see cref="GenericHttpRequest"/>s.
    /// </summary>
    public static class AbstractHttpRequestHelper
    {
        /// <summary>
        ///   Constructs and returns a <see cref="GenericHttpRequest"/> representation of a
        ///   <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">
        ///   The request to be represented as a <see cref="GenericHttpRequest"/>.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking a request/response (mainly for diagnostics purposes).
        /// </param>
        /// <param name="contentAsString">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies that request content should be recorded as a string.
        ///   This is only useful for tracing purposes.
        /// </param>
        /// <returns>
        ///   A <see cref="GenericHttpRequest"/>
        /// </returns>
        public static async Task<GenericHttpRequest> ToGenericHttpRequestAsync(
            this HttpRequestMessage request, 
            string? messageId = null,
            bool contentAsString = false)
            => new()
            {
                MessageId = messageId,
                Method = request.Method.Method,
                Uri = request.RequestUri,
                Headers = request.Headers,
                Content = request.Content is {} ? await request.Content.ReadAsStreamAsync() : null,
                ContentAsString = contentAsString && request.Content is {} ? await request.Content.ReadAsStringAsync() : null
            };

        /// <summary>
        ///   Constructs and returns a <see cref="GenericHttpRequest"/> representation of a
        ///   <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="request">
        ///   The request to be represented as a <see cref="GenericHttpRequest"/>.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking a request/response (mainly for diagnostics purposes).
        /// </param>
        /// <returns>
        ///   A <see cref="GenericHttpRequest"/>
        /// </returns>
        public static Task<GenericHttpRequest> ToGenericHttpRequestAsync(
            this HttpRequest request, 
            string? messageId = null) 
            => Task.FromResult(new GenericHttpRequest
                {
                    MessageId = messageId,
                    Method = request.Method,
                    Uri = request.GetUri(),
                    Headers = request.Headers.ToKeyValuePairs(),
                    Content = request.Body
                });
        
        /// <summary>
        ///   Constructs and returns a <see cref="GenericHttpRequest"/> representation of a
        ///   <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="request">
        ///   The request to be represented as a <see cref="GenericHttpRequest"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="GenericHttpRequest"/>
        /// </returns>
        public static Task<GenericHttpRequest> ToGenericHttpRequestAsync(this HttpWebRequest request)
            => Task.FromResult(new GenericHttpRequest
            {
                Method = request.Method,
                Uri = request.RequestUri,
                Headers = request.Headers.ToKeyValuePairs(),
                Content = request.GetRequestStream()
            });
        
        /// <summary>
        ///   Constructs and returns the textual representation of the <see cref="HttpRequest"/>'s URI (if any). 
        /// </summary>
        /// <param name="request">
        ///   The <see cref="HttpRequest"/> to obtain the URI from.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> representation of the request's URI, if any; otherwise <c>null</c>.
        /// </returns>
        public static Uri? GetUri(this HttpRequest? request) 
        {
            var uri = request?.GetDisplayUrl();
            return uri is { } ? new Uri(uri) : null;
        }
    }
}