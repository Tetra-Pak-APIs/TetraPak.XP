using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.Web.Abstractions
{
    /// <summary>
    ///   Convenient methods for working with <see cref="GenericHttpResponse"/>s.
    /// </summary>
    public static class GenericHttpResponseHelper
    {
        /// <summary>
        ///   Constructs and returns a <see cref="GenericHttpResponse"/> representation of a
        ///   <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="response">
        ///   The response to be represented as a <see cref="GenericHttpResponse"/>.
        /// </param>
        /// <param name="messageId">
        ///   (optional)<br/>
        ///   A unique string value for tracking a request/response (mainly for diagnostics purposes).
        /// </param>
        /// <returns>
        ///   A <see cref="GenericHttpResponse"/>
        /// </returns>
        public static async Task<GenericHttpResponse> ToGenericHttpResponseAsync(
            this HttpResponseMessage response, 
            string? messageId = null,
            bool contentAsString = false)
            => new()
            {
                MessageId = messageId,
                StatusCode = (int) response.StatusCode,
                Method = response.RequestMessage?.Method.Method!,
                Uri = response.RequestMessage?.RequestUri,
                Headers = response.Headers,
                Content = await response.Content.ReadAsStreamAsync(),
                ContentAsString = contentAsString ? await response.Content.ReadAsStringAsync() : null
            };

        /// <summary>
        ///   Constructs and returns a <see cref="GenericHttpResponse"/> representation of a
        ///   <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="response">
        ///   The response to be represented as a <see cref="GenericHttpResponse"/>.
        /// </param>
        /// <param name="request">
        ///   (optional)<br/>
        ///   A request (resulting in the response). 
        /// </param>
        /// <returns>
        ///   A <see cref="GenericHttpResponse"/>
        /// </returns>
        public static async Task<GenericHttpResponse> ToGenericHttpResponseAsync(
            this HttpResponse response, 
            GenericHttpRequest? request)
        {
            request ??= 
                await response.HttpContext.Request.ToGenericHttpRequestAsync(
                    response.HttpContext.Request.GetMessageId(null));
            return new GenericHttpResponse
            {
                MessageId = request.MessageId,
                StatusCode = response.StatusCode,
                Method = request.Method,
                Uri = request.Uri,
                Headers = response.Headers.ToKeyValuePairs(),
                Content = response.Body
            };
        }

        /// <summary>
        ///   Constructs and returns a <see cref="GenericHttpResponse"/> representation of a
        ///   <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="response">
        ///   The response to be represented as a <see cref="GenericHttpResponse"/>.
        /// </param>
        /// <param name="request">
        ///   (optional)<br/>
        ///   A request (resulting in the response). 
        /// </param>
        /// <returns>
        ///   A <see cref="GenericHttpResponse"/>
        /// </returns>
        public static GenericHttpResponse ToGenericHttpResponse(
            this WebResponse response,
            GenericHttpRequest? request = null)
        {
            return new GenericHttpResponse
            {
                StatusCode = response is HttpWebResponse webResponse ? (int)webResponse.StatusCode : 0,
                Method = request?.Method!,
                Uri = request?.Uri,
                Headers = response.Headers.ToKeyValuePairs(),
            };
        }
    }
}