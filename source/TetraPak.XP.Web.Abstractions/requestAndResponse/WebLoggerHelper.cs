using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using TetraPak.XP.Streaming;
using TetraPak.XP.Web.Abstractions;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP.Web.Http
{
    partial class WebLoggerHelper // HTTP traffic
    {
        /// <summary>
        ///   Attempts building and returning a textual representation of a <see cref="Stream"/>. 
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="Stream"/> to be textually represented.
        /// </param>
        /// <param name="encoding">
        ///   The character <see cref="Encoding"/> to be used.
        /// </param>
        /// <param name="options">
        ///   (optional)<br/>
        ///   Options for how tracing is conducted. 
        /// </param>
        /// <returns></returns>
        public static async Task<string> GetRawBodyStringAsync( // todo Consider skipping this for .NET Standard 2.0 (cannot retain stream)
            this Stream? stream, 
            Encoding encoding, 
            AbstractTraceHttpMessageOptions? options = null)
        {
            var canSeek = stream!.CanSeek;
            if (!canSeek)
                return "[*** BODY UNAVAILABLE (stream not seekable) ***]";

            var lengthOutcome = await stream.GetLengthAsync(options?.ForceTraceBody ?? AbstractTraceHttpMessageOptions.DefaultForceTraceBody);
            var length = lengthOutcome.Value;
            if (length == 0)
                return string.Empty;
                
            var bufferSize = (int) Math.Min(options?.BufferSize ?? AbstractTraceHttpMessageOptions.DefaultBuffersSize, length);
            string body;
            if (options is null || length <= options.MaxSize)
            {
                // no need to read in chunks, just get and return the whole body ...
                using var smallBodyReader = new StreamReader(
                    stream, 
                    encoding, 
                    true, 
                    bufferSize, 
                    true);
                body = await smallBodyReader.ReadToEndAsync();
                stream.Position = 0;
                return body;
            }
            
            // read body in chunks of buffer size and truncate if there's a max length
            // (to avoid performance issues with very large bodies, such as media or binaries) ... 
            var buffer = new char[bufferSize];
            var isTruncated = options.MaxSize < length;
            var bodyStream = new MemoryStream();
            var writer = new StreamWriter(bodyStream);
            StreamReader? memoryReader = null;
            try
            {
                var largeBodyReader = new StreamReader(stream, encoding, true, bufferSize, true);
                int readCount;
                do
                {
                    readCount = await largeBodyReader.ReadBlockAsync(buffer, 0, bufferSize);
                    await writer.WriteAsync(buffer);
                
                } while (readCount == bufferSize);
                stream.Position = 0; 
                bodyStream.Position = 0;
                memoryReader = new StreamReader(bodyStream); // leave open (`writer` will close)
                body = await memoryReader.ReadToEndAsync();
                return isTruncated
                    ? $"{body}...[--TRUNCATED--]"
                    : body;
            }
            finally
            {
#if NET5_0_OR_GREATER                
                await writer.DisposeAsync();
#else
                writer.Dispose();
#endif
                memoryReader?.Dispose();
            }
        }
        
        static void addHeaders(
            StringBuilder sb, 
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? defaultHeaders)
        {
            var defaultHeadersArray = defaultHeaders?.ToArray();
            var requestHeadersArray = requestHeaders.ToArray();
            if ((defaultHeadersArray?.Length ?? 0) != 0)
            {
                var reqDict = requestHeadersArray.ToDictionary(i => i.Key);
                for (var i = 0; i < defaultHeadersArray!.Length; i++)
                {
                    var key = defaultHeadersArray[i].Key;
                    if (!reqDict.ContainsKey(key))
                    {
                        append(key, defaultHeadersArray[i].Value);
                    }
                }
            }

            for (var i = 0; i < requestHeadersArray.Length; i++)
            {
                append(requestHeadersArray[i].Key, requestHeadersArray[i].Value);
                
            }

            void append(string key, IEnumerable<string> values)
            {
                var valuesArray = values.ToArray();
                if (!valuesArray.Any())
                {
                    sb.AppendLine(key);
                    return;
                }
            
                sb.Append(key);
                sb.Append('=');
                sb.AppendLine(valuesArray.ConcatEnumerable());
            }
        }

        /// <summary>
        ///   Converts a dictionary of <see cref="StringValues"/> (such as <see cref="IHeaderDictionary"/>)
        ///   into a collection of key-value pairs with values as <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="dict">
        ///   The dictionary to be converted.
        /// </param>
        /// <returns>
        ///   A <see cref="IEnumerable{T}"/> collection.
        /// </returns>
        /// <remarks>
        ///   This method is used internally to convert the various classes representing HTTP requests or responses
        ///   into an <see cref="GenericHttpRequest"/> or 
        /// </remarks>
        public static IEnumerable<KeyValuePair<string, IEnumerable<string>>> ToKeyValuePairs(
            this IDictionary<string, global::Microsoft.Extensions.Primitives.StringValues> dict)
        {
            return dict.Select(pair => new KeyValuePair<string, IEnumerable<string>>(pair.Key, pair.Value));
        }
        
        /// <summary>
        ///   Converts a dictionary of <see cref="StringValues"/> (such as <see cref="IHeaderDictionary"/>)
        ///   into a collection of key-value pairs with values as <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="collection">
        ///   The collection to be converted.
        /// </param>
        /// <returns>
        ///   A <see cref="IEnumerable{T}"/> collection.
        /// </returns>
        /// <remarks>
        ///   This method is used internally to convert the various classes representing HTTP requests or responses
        ///   into an <see cref="GenericHttpRequest"/> or 
        /// </remarks>
        public static IEnumerable<KeyValuePair<string, IEnumerable<string>>> ToKeyValuePairs(
            this NameValueCollection collection)
        {
            foreach (string? key in collection)
            {
                if (key is null)
                    continue;
                
                var values = collection.GetValues(key) ?? Array.Empty<string>();
                yield return new KeyValuePair<string, IEnumerable<string>>(key, values);
            }
        }
    }
}