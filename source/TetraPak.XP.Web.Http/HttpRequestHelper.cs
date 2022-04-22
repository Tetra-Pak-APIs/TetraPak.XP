using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TetraPak.XP.Web.Http
{

    /// <summary>
    ///   Provides convenient methods for working with HTTP request objects.
    /// </summary>
    public static class HttpRequestHelper
    {
        /// <summary>
        ///   Clones the <see cref="HttpRequestMessage"/>. 
        /// </summary>
        /// <param name="message">
        ///   The <see cref="HttpRequestMessage"/> to be cloned.
        /// </param>
        /// <returns>
        ///   The cloned <see cref="HttpRequestMessage"/>.
        /// </returns>
        public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage message)
        {
            var clone = new HttpRequestMessage(message.Method, message.RequestUri)
            {
                Version = message.Version
            };

            // copy content via a MemoryStream ...
            var stream = new MemoryStream();
            if (message.Content is { })
            {
                await message.Content.CopyToAsync(stream).ConfigureAwait(false);
                stream.Position = 0;
                clone.Content = new StreamContent(stream);

                // copy content headers ...
                if (message.Content.Headers.Any())
                {
                    foreach (var header in message.Content.Headers)
                    {
                        clone.Content.Headers.Add(header.Key, header.Value);
                    }
                }
            }

#if NET5_0_OR_GREATER
            var clonedOptionsDictionary = (IDictionary<string, object?>)clone.Options;
            foreach (var (key, value) in message.Options)
            {
                clonedOptionsDictionary.Add(key, value);
            }
#else
            foreach (var pair in message.Properties)
            {
                clone.Properties.Add(pair.Key, pair.Value);
            }
#endif
            foreach (var pair in message.Headers)
            {
                clone.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }

            return clone;
        }

        /// <summary>
        ///   Resets the <see cref="HttpHeaderValueCollection{T}"/> to a specified set of headers.
        /// </summary>
        /// <param name="self">
        ///   The headers collection to be reset.
        /// </param>
        /// <param name="headers">
        ///   The headers to be assigned.
        /// </param>
        /// <typeparam name="T">
        ///   The type of headers supported by the headers collection.
        /// </typeparam>
        public static void ResetTo<T>(this HttpHeaderValueCollection<T> self, params T[] headers)
            where T : class
        {
            self.Clear();
            foreach (var header in headers)
            {
                self.Add(header);
            }
        }
    }
}