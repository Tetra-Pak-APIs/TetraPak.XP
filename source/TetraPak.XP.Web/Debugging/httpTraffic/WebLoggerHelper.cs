using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using TetraPak.Serialization;

namespace TetraPak.AspNet.Debugging
{
    partial class WebLoggerHelper // HTTP traffic
    {
        static bool s_isTraceRequestAdded;
        static TraceRequestMiddleware? s_traceRequestMiddleware;
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Injects middleware that traces all requests to the logger provider
        ///   when <see cref="LogLevel.Trace"/> is set.
        /// </summary>
        /// <param name="app">
        ///   The extended application builder. 
        /// </param>
        public static IApplicationBuilder UseTetraPakTraceRequestAsync(this IApplicationBuilder app)
        {
            lock (s_syncRoot)
            {
                if (s_isTraceRequestAdded)
                    return app;

                s_isTraceRequestAdded = true;
                var config = app.ApplicationServices.GetRequiredService<TetraPakConfig>();
                var logger = app.ApplicationServices.GetService<ILogger<TraceRequestMiddleware>>();
                if (logger is null || !logger.IsEnabled(LogLevel.Trace))
                    return app;

                s_traceRequestMiddleware = new TraceRequestMiddleware(config, logger);
                app.Use(async (context, next) =>
                {
                    await s_traceRequestMiddleware.InvokeAsync(context);
                    await next();
                });
            }

            return app;
        }
        
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
        public static async Task<string> GetRawBodyStringAsync(
            this Stream? stream, 
            Encoding encoding, 
            AbstractTraceHttpMessageOptions? options = null)
        {
            var lengthOutcome = await stream.GetLengthAsync(options?.ForceTraceBody ?? AbstractTraceHttpMessageOptions.DefaultForceTraceBody);
            var length = lengthOutcome.Value;
            if (length == 0)
                return string.Empty;

            if (!stream!.CanSeek)
                return "[*** BODY UNAVAILABLE ***]";
                
            var bufferSize = options?.BufferSize ?? AbstractTraceHttpMessageOptions.DefaultBuffersSize;

            string body;
            if (options is null || length <= options.MaxSize)
            {
                // no need to read in chunks, just get and return the whole body ...
                using var bodyReader = new StreamReader(stream, encoding, true, 
                    bufferSize, 
                    true);
                body = await bodyReader.ReadToEndAsync();
                stream.Position = 0;
                return body;
            }
            
            // read body in chunks of buffer size and truncate if there's a max length
            // (to avoid performance issues with very large bodies, such as media or binaries) ... 
            var buffer = new char[bufferSize];
            var remaining = options.MaxSize;
            var isTruncated = false;

            var bodyStream = new MemoryStream();
            await using var writer = new StreamWriter(bodyStream);
            using var reader = new StreamReader(stream, encoding, true, bufferSize, true);
            
            while (await reader.ReadBlockAsync(buffer) != 0 && !isTruncated)
            {
                await writer.WriteAsync(buffer);
                remaining -= bufferSize;
                isTruncated = remaining <= 0; 
            }
            bodyStream.Position = 0;
            using (var memoryReader = new StreamReader(bodyStream, leaveOpen:true)) // leave open (`writer` will close)
            {
                body = await memoryReader.ReadToEndAsync();
            }
            stream.Position = 0;
            return isTruncated
                ? $"{body}...[--TRUNCATED--]"
                : body;
        }
        
        // static async Task traceAsync( obsolete
        //     ILogger? logger, 
        //     WebResponse? response, 
        //     Func<WebResponse?,Task<string>>? bodyHandler)
        // {
        //     if (logger is null || response is null || !logger.IsEnabled(LogLevel.Debug))
        //         return;
        //     
        //     var sb = new StringBuilder();
        //     if (response is HttpWebResponse webResponse)
        //     {
        //         sb.Append((int)webResponse.StatusCode);
        //         sb.Append(' ');
        //         sb.AppendLine(webResponse.StatusCode.ToString());
        //     }
        //     else
        //     {
        //         sb.AppendLine("(status code unavailable)");
        //     }
        //     addHeaders(sb, response.Headers.ToKeyValuePairs(), null);
        //     await addBody();
        //
        //     logger.Trace(sb.ToString());
        //     
        //     async Task addBody()
        //     {
        //         if (bodyHandler is null)
        //             return;
        //         
        //         sb.AppendLine();
        //         sb.AppendLine(await bodyHandler(response));
        //     }
        // }
        
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
                sb.AppendLine(valuesArray.ConcatCollection());
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
            this IDictionary<string, StringValues> dict)
        {
            foreach (var (key, values) in dict)
            {
                yield return new KeyValuePair<string, IEnumerable<string>>(key, values);
            }
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