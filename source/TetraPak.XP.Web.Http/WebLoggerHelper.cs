using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Internal;
using TetraPak.XP.Logging;
using TetraPak.XP.Streaming;
using TetraPak.XP.Web.Http.Debugging;
using HttpMethod=Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace TetraPak.XP.Web.Http
{
    /// <summary>
    ///   Contains convenience/extension methods to assist with logging. 
    /// </summary>
    public static partial class WebLoggerHelper
    {
        static bool s_isAuthConfigAlreadyLogged;
        static readonly object s_syncRoot = new();

        /// <summary>
        ///   Gets or sets a threshold value used when tracing HTTP traffic. When traffic size
        ///   exceeds this value the tracing will automatically be delegated to a background thread. 
        /// </summary>
        public static int TraceThreshold { get; set; } = 2048;

        internal static async Task<bool> ExceedsTraceThresholdAsync(this Stream? stream)
        {
            if (TraceThreshold <= 0 || stream is null)
                return false;
                
            try
            {
                return TraceThreshold >= 0 && await stream.GetLengthAsync() > TraceThreshold;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        ///   Logs all assemblies currently in use by the process.
        /// </summary>
        /// <param name="log">
        ///   The logger provider.
        /// </param>
        public static Task DebugAssembliesInUseAsync(this ILog? log)
        {
            return Task.Run(() =>
            {
                log.Debug(() =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(">===== ASSEMBLIES =====<");
                    sb.appendAssembliesInUse();
                    sb.AppendLine(">======================<");
                    return sb.ToString();
                });
            });
        }

        static void appendAssembliesInUse(this StringBuilder sb)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                sb.AppendLine(assembly.FullName);
            }
        }

        // /// <summary>
        // ///   Builds and a state dump from a <see cref="TetraPakConfig"/> object and writes it to the logger.  obsolete
        // /// </summary>
        // /// <param name="logger">
        // ///   The extended logger provider.
        // /// </param>
        // /// <param name="tetraPakConfig">
        // ///   The <see cref="TetraPakConfig"/> object to be state dumped to the logger. 
        // /// </param>
        // /// <param name="logLevel">
        // ///   (optional; default=<see cref="Trace"/>)<br/>
        // ///   A (custom) log level for the state dump information.
        // /// </param>
        // /// <param name="justOnce">
        // ///   (optional; default=<c>true</c>)<br/>
        // ///   When set the state dump will only be performed once.
        // ///   The state dump will be ignored if invoked again and this value was set previously (and now).  
        // /// </param>
        // public static async Task LogTetraPakConfigAsync(
        //     this ILog? logger, 
        //     TetraPakConfig? tetraPakConfig, 
        //     LogLevel logLevel = LogLevel.Trace,
        //     bool justOnce = true)
        // {
        //     if (tetraPakConfig is null || !(logger?.IsEnabled(logLevel) ?? false))
        //         return;
        //
        //     lock (s_syncRoot)
        //     {
        //         if (justOnce && s_isAuthConfigAlreadyLogged)
        //             return;
        //
        //         s_isAuthConfigAlreadyLogged = true;
        //     }
        //     
        //     var stateDump = new StateDump("Tetra Pak Configuration", logger);
        //     await stateDump.AddAsync(tetraPakConfig, "TetraPak");
        //
        //     logger.LogLevel(await stateDump.BuildAsStringAsync(), logLevel);
        // }

        public static Task<StringBuilder> ToStringBuilderAsync(
            this Uri uri,
            string? initiator,
            StringBuilder? stringBuilder,
            HttpMethod httpMethod = HttpMethod.Get,
            HttpDirection direction = HttpDirection.Out)
        {
            stringBuilder ??= new StringBuilder();
            var qualifier = TraceRequest.GetTraceRequestQualifier(direction, initiator, null);
            stringBuilder.Append(qualifier);
            stringBuilder.Append("  ");
            stringBuilder.Append(httpMethod.ToString());
            stringBuilder.Append("  ");
            stringBuilder.AppendLine(uri.ToString());
            return Task.FromResult(stringBuilder);
        }
        
        /// <summary>
        ///   Builds a textual representation of the <see cref="GenericHttpRequest"/>.
        /// </summary>
        /// <param name="request">
        ///   The <see cref="GenericHttpRequest"/> to be textually represented.
        /// </param>
        /// <param name="stringBuilder">
        ///   The <see cref="StringBuilder"/> to be used.
        /// </param>
        /// <param name="optionsFactory">
        ///   (optional)<br/>
        ///   Provides <see cref="TraceHttpRequestOptions"/> specifying how to build the textual representation
        ///   of the <see cref="GenericHttpRequest"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="StringBuilder"/> that contains the textual representation
        ///   of the <see cref="GenericHttpRequest"/>.
        /// </returns>
        public static async Task<StringBuilder> ToStringBuilderAsync(
            this GenericHttpRequest request,
            StringBuilder stringBuilder,
            Func<TraceHttpRequestOptions>? optionsFactory = null)
        {
            var options = optionsFactory?.Invoke();

            var qualifier = TraceRequest.GetTraceRequestQualifier(
                options?.Direction ?? HttpDirection.Unknown, 
                options?.Initiator, 
                options?.Detail);
            if (!string.IsNullOrEmpty(qualifier))
            {
                stringBuilder.AppendLine(qualifier);
            }

            stringBuilder.AppendLine();
            stringBuilder.Append(request.Method.ToUpper());
            stringBuilder.Append(' ');
            var requestUri = request.Uri is {} 
                ? options?.BaseAddress is { } 
                    ? request.Uri.ToString().TrimStart('/')
                    : request.Uri.AbsoluteUri
                : string.Empty;
            var uri = options?.BaseAddress is { }
                ? $"{options.BaseAddress.ToString().EnsurePostfix("/")}{requestUri}"
                : requestUri;
            stringBuilder.AppendLine(uri);
            stringBuilder.AppendLine();
            addHeaders(stringBuilder, request.Headers, options?.DefaultHeaders);
            await addBodyAsync(stringBuilder);
            return options?.AsyncDecorationHandler is { }
                ? await options.AsyncDecorationHandler(stringBuilder)
                : stringBuilder;
            
            async Task addBodyAsync(StringBuilder sb)
            {
                if (options?.AsyncBodyFactory is {})
                {
                    sb.AppendLine();
                    sb.AppendLine(await options.AsyncBodyFactory());
                    return;
                }

                if (request.Content is null)
                    return;
                
                var bodyText = request.ContentAsString 
                               ?? await request.Content.GetRawBodyStringAsync(Encoding.Default, options);
                if (string.IsNullOrEmpty(bodyText))
                    return;
                
                sb.AppendLine();
                sb.AppendLine(bodyText);
            }
        }
        
        /// <summary>
        ///   Builds a textual representation of the <see cref="GenericHttpResponse"/>.
        /// </summary>
        /// <param name="response">
        ///   The <see cref="GenericHttpResponse"/> to be textually represented.
        /// </param>
        /// <param name="stringBuilder">
        ///   The <see cref="StringBuilder"/> to be used.
        /// </param>
        /// <param name="optionsFactory">
        ///   (optional)<br/>
        ///   Provides <see cref="TraceHttpRequestOptions"/> specifying how to build the textual representation
        ///   of the <see cref="GenericHttpResponse"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="StringBuilder"/> that contains the textual representation
        ///   of the <see cref="GenericHttpResponse"/>.
        /// </returns>
        public static async Task<StringBuilder> ToStringBuilderAsync(
            this GenericHttpResponse response,
            StringBuilder stringBuilder,
            Func<TraceHttpResponseOptions>? optionsFactory = null)
        {
            var options = (optionsFactory?.Invoke() ?? TraceHttpResponseOptions.Default()).WithDirection(HttpDirection.Response);

            // trace qualifier (eg. "{initiator} >>> IN (detail) >>>")
            var qualifier = TraceRequest.GetTraceRequestQualifier(
                options.Direction, 
                options.Initiator, 
                options.Detail);
            if (!string.IsNullOrEmpty(qualifier))
            {
                stringBuilder.AppendLine(qualifier);
                stringBuilder.AppendLine();
            }

            // HTTP method (optional)
            if (!string.IsNullOrEmpty(response.Method))
            {
                stringBuilder.Append(response.Method.ToUpper());
                stringBuilder.Append(' ');
            }

            // request Uri (optional)
            if (response.Uri is { } && !options.HideRequestUri)
            {
                var requestUri = response.Uri is {} 
                    ? options.BaseAddress is { } 
                        ? response.Uri.ToString().TrimStart('/')
                        : response.Uri.AbsoluteUri
                    : string.Empty;
                var uri = options.BaseAddress is { }
                    ? $"{options.BaseAddress.ToString().EnsurePostfix("/")}{requestUri}"
                    : requestUri;
                stringBuilder.AppendLine(uri);
            }

            addStatusCode();
            addHeaders(stringBuilder, response.Headers, options.DefaultHeaders);
            await addBodyAsync(stringBuilder);
            return options.AsyncDecorationHandler is { }
                ? await options.AsyncDecorationHandler(stringBuilder)
                : stringBuilder;
            
            void addStatusCode()
            {
                if (response.StatusCode == 0)
                {
                    stringBuilder.AppendLine("(NO STATUS CODE)");
                    stringBuilder.AppendLine();
                    return;
                }

                stringBuilder.AppendLine($"{response.StatusCode.ToString()} {(HttpStatusCode)response.StatusCode}");
                stringBuilder.AppendLine();
            }
            
            async Task addBodyAsync(StringBuilder sb)
            {
                if (options.AsyncBodyFactory is {})
                {
                    sb.AppendLine();
                    sb.AppendLine(await options.AsyncBodyFactory());
                    return;
                }

                if (response.Content is null)
                    return;
                
                var bodyText = response.ContentAsString 
                               ?? await response.Content.GetRawBodyStringAsync(Encoding.Default, options);
                if (string.IsNullOrEmpty(bodyText))
                    return;
                
                sb.AppendLine();
                sb.AppendLine(bodyText);
            }
        }

        /// <summary>
        ///   Builds a textual representation of an <see cref="GenericHttpRequest"/> and logs it at 
        ///   log level <see cref="Trace"/>
        /// </summary>
        /// <param name="log">
        ///   The logger provider.
        /// </param>
        /// <param name="request">
        ///   The request to be traced.
        /// </param>
        /// <param name="optionsFactory">
        ///   (optional)<br/>
        ///   Invoked to obtain options for how tracing is conducted.
        /// </param>
        public static async Task TraceAsync(
            this ILog? log,
            GenericHttpRequest request,
            Func<TraceHttpRequestOptions>? optionsFactory)
        {
            if (log is null || !log.IsEnabled(LogRank.Trace))
                return;
                
            var sb = await request.ToStringBuilderAsync(new StringBuilder(), optionsFactory);
            var options = optionsFactory?.Invoke();
            log.Trace(sb.ToString(), options?.MessageId);
        }

        /// <summary>
        ///   Builds a textual representation of an <see cref="GenericHttpResponse"/> and logs it at 
        ///   log level <see cref="Trace"/>
        /// </summary>
        /// <param name="log">
        ///   The logger provider.
        /// </param>
        /// <param name="response">
        ///   The response to be traced.
        /// </param>
        /// <param name="optionsFactory">
        ///   (optional)<br/>
        ///   Invoked to obtain options for how tracing is conducted.
        /// </param>
        public static async Task TraceAsync(
            this ILog? log,
            GenericHttpResponse response,
            Func<TraceHttpResponseOptions>? optionsFactory)
        {
            if (log is null || !log.IsEnabled(LogRank.Trace))
                return;
                
            var sb = await response.ToStringBuilderAsync(new StringBuilder(), optionsFactory);
            var options = optionsFactory?.Invoke();
            log.Trace(sb.ToString(), options?.MessageId);
        }
    }
}