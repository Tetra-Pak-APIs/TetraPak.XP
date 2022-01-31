using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TetraPak.AspNet.Debugging
{
    /// <summary>
    ///   Used to control how HTTP request gets represented (for tracing).
    /// </summary>
    public class TraceHttpRequestOptions : AbstractTraceHttpMessageOptions
    {
        /// <summary>
        ///   Gets default <see cref="TraceHttpRequestOptions"/>
        /// </summary>
        public static TraceHttpRequestOptions Default(string? messageId) => new(messageId);
        
        #region .  Fluent API  .

        /// <summary>
        ///   (fluent api)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.Direction"/> property and returns <c>this</c>.
        /// </summary>
        /// <param name="value">
        ///   The <see cref="AbstractTraceHttpMessageOptions.Initiator"/> value.
        /// </param>
        /// <param name="direction">
        ///   (optional)<br/>
        ///   Assign this value to also invoke <see cref="WithDirection(HttpDirection,string?)"/>.
        /// </param>
        /// <seealso cref="AbstractTraceHttpMessageOptions.Initiator"/>
        /// <seealso cref="WithDirection(HttpDirection,string?)"/>
        /// <seealso cref="WithDirection(HttpDirection,object?)"/>
        public TraceHttpRequestOptions WithInitiator(string value, HttpDirection? direction) 
            => WithInitiator<TraceHttpRequestOptions>(value, direction);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.Direction"/> property and returns <c>this</c>.
        /// </summary>
        /// <param name="initiator">
        ///   The <see cref="AbstractTraceHttpMessageOptions.Initiator"/> value.
        /// </param>
        /// <param name="direction">
        ///   (optional)<br/>
        ///   Assign this value to also invoke <see cref="WithDirection(HttpDirection,object?)"/>.
        /// </param>
        /// <seealso cref="AbstractTraceHttpMessageOptions.Initiator"/>
        /// <seealso cref="WithDirection(HttpDirection,string?)"/>
        /// <seealso cref="WithDirection(HttpDirection,object?)"/>
        public TraceHttpRequestOptions WithInitiator(object initiator, HttpDirection? direction)  
            => WithInitiator<TraceHttpRequestOptions>(initiator.ToString() ?? initiator.GetType().ToString(), direction);
        
        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.Direction"/> property and returns <c>this</c>.
        /// </summary>
        /// <param name="initiator">
        ///   The <see cref="AbstractTraceHttpMessageOptions.Initiator"/> value.
        /// </param>
        /// <seealso cref="AbstractTraceHttpMessageOptions.Initiator"/>
        /// <seealso cref="WithDirection(HttpDirection,string?)"/>
        /// <seealso cref="WithDirection(HttpDirection,object?)"/>
        public TraceHttpRequestOptions WithInitiator(object initiator)
            => WithInitiator<TraceHttpRequestOptions>(
                initiator.ToString() ?? initiator.GetType().ToString(), 
                null);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.Direction"/> property and returns <c>this</c>.
        /// </summary>
        /// <param name="value">
        ///   The <see cref="HttpDirection"/> value.
        /// </param>
        /// <param name="initiator">
        ///   (optional)<br/>
        ///   Assign this value to also invoke <see cref="WithInitiator(string,HttpDirection?)"/>.
        /// </param>
        /// <seealso cref="AbstractTraceHttpMessageOptions.Direction"/>
        /// <seealso cref="WithInitiator(string,HttpDirection?)"/>
        /// <seealso cref="WithInitiator(object,HttpDirection?)"/>
        public TraceHttpRequestOptions WithDirection(HttpDirection value, string? initiator)
            => WithInitiator<TraceHttpRequestOptions>(initiator, value);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.Direction"/> property and returns <c>this</c>.
        /// </summary>
        /// <param name="value">
        ///   The <see cref="HttpDirection"/> value.
        /// </param>
        /// <param name="initiator">
        ///   (optional)<br/>
        ///   Assign this value to also invoke <see cref="WithInitiator(object,HttpDirection?)"/>.
        /// </param>
        /// <seealso cref="AbstractTraceHttpMessageOptions.Direction"/>
        /// <seealso cref="WithInitiator(string,HttpDirection?)"/>
        /// <seealso cref="WithInitiator(object,HttpDirection?)"/>
        public TraceHttpRequestOptions WithDirection(HttpDirection value, object? initiator) 
            => WithDirection<TraceHttpRequestOptions>(value, initiator?.ToString() ?? initiator?.GetType().ToString());
        
        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.Direction"/> property and returns <c>this</c>.
        /// </summary>
        /// <param name="value">
        ///   The <see cref="HttpDirection"/> value.
        /// </param>
        /// <seealso cref="AbstractTraceHttpMessageOptions.Direction"/>
        /// <seealso cref="WithInitiator(string,HttpDirection?)"/>
        /// <seealso cref="WithInitiator(object,HttpDirection?)"/>
        public TraceHttpRequestOptions WithDirection(HttpDirection value) 
            =>
            WithDirection<TraceHttpRequestOptions>(value, null);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.Detail"/> property and returns <c>this</c>.
        /// </summary>
        /// <seealso cref="AbstractTraceHttpMessageOptions.Detail"/>
        public TraceHttpRequestOptions WithDetail(string value)
            =>
            WithDetail<TraceHttpRequestOptions>(value);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.BaseAddress"/> property and returns <c>this</c>.
        /// </summary>
        public TraceHttpRequestOptions WithBaseAddress(Uri baseAddress)
            =>
            WithBaseAddress<TraceHttpRequestOptions>(baseAddress);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.DefaultHeaders"/> property and returns <c>this</c>.
        /// </summary>
        public TraceHttpRequestOptions WithDefaultHeaders(HttpRequestHeaders headers)
            =>
            WithDefaultHeaders<TraceHttpRequestOptions>(headers);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.AsyncBodyFactory"/> property and returns <c>this</c>.
        /// </summary>
        public TraceHttpRequestOptions WithAsyncBodyFactory(Func<Task<string>>? factory)
            => WithAsyncBodyFactory<TraceHttpRequestOptions>(factory);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.AsyncDecorationHandler"/>
        ///   decorator and returns <c>this</c>.
        /// </summary>
        public TraceHttpRequestOptions WithDecorator(Func<StringBuilder, Task<StringBuilder>> decorator)
            => WithDecorator<TraceHttpRequestOptions>(decorator);
        
        #endregion // Fluent API    
        
        internal TraceHttpRequestOptions(string? messageId = null)
        : base(messageId)
        {
        }
    }
}