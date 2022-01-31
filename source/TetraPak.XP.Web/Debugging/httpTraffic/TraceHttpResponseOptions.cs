using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TetraPak.AspNet.Debugging
{
    /// <summary>
    ///   Used to control how HTTP response gets represented (for tracing).
    /// </summary>
    public class TraceHttpResponseOptions : AbstractTraceHttpMessageOptions
    {
        /// <summary>
        ///   Gets default <see cref="TraceHttpRequestOptions"/>
        /// </summary>
        public static TraceHttpResponseOptions Default(string? messageId = null)
        {
            return new TraceHttpResponseOptions(messageId)
            {
                Direction = HttpDirection.Response
            }
            .WithRequestUri(false);
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Assigns the <see cref="TraceHttpRequestOptions.HideRequestUri"/> and returns <c>this</c>.
        /// </summary>
        /// <param name="value">
        ///   (optional; default=<c>true</c>)<br/>
        ///   Assigns the <see cref="TraceHttpRequestOptions.HideRequestUri"/> to the negated assigned value
        ///   (pass <c>false</c> to actually hide the <see cref="GenericHttpRequest.Uri"/>).   
        /// </param>
        public TraceHttpResponseOptions WithRequestUri(bool value = true)
        {
            HideRequestUri = !value;
            return this;
        }

        #region .  Fluent API  .
        /// <summary>
        ///   (fluent API)<br/>
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
        public TraceHttpResponseOptions WithInitiator(string value, HttpDirection? direction) 
            => WithInitiator<TraceHttpResponseOptions>(value, direction);

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
        public TraceHttpResponseOptions WithInitiator(object initiator, HttpDirection? direction)  
            => WithInitiator<TraceHttpResponseOptions>(initiator.ToString() ?? initiator.GetType().ToString(), direction);
        
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
        public TraceHttpResponseOptions WithInitiator(object initiator)
            => WithInitiator<TraceHttpResponseOptions>(
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
        public TraceHttpResponseOptions WithDirection(HttpDirection value, string? initiator)
            => WithInitiator<TraceHttpResponseOptions>(initiator, value);

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
        public TraceHttpResponseOptions WithDirection(HttpDirection value, object? initiator) 
            => WithDirection<TraceHttpResponseOptions>(value, initiator?.ToString() ?? initiator?.GetType().ToString());
        
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
        public TraceHttpResponseOptions WithDirection(HttpDirection value) 
            => WithDirection<TraceHttpResponseOptions>(value, null);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.Detail"/> property and returns <c>this</c>.
        /// </summary>
        /// <seealso cref="AbstractTraceHttpMessageOptions.Detail"/>
        public TraceHttpResponseOptions WithDetail(string value)
            =>
            WithDetail<TraceHttpResponseOptions>(value);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.BaseAddress"/> property and returns <c>this</c>.
        /// </summary>
        public TraceHttpResponseOptions WithBaseAddress(Uri baseAddress)
            =>
            WithBaseAddress<TraceHttpResponseOptions>(baseAddress);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.DefaultHeaders"/> property and returns <c>this</c>.
        /// </summary>
        public TraceHttpResponseOptions WithDefaultHeaders(HttpRequestHeaders headers)
            =>
            WithDefaultHeaders<TraceHttpResponseOptions>(headers);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.AsyncBodyFactory"/> property and returns <c>this</c>.
        /// </summary>
        public TraceHttpResponseOptions WithAsyncBodyFactory(Func<Task<string>>? factory)
            => WithAsyncBodyFactory<TraceHttpResponseOptions>(factory);

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="AbstractTraceHttpMessageOptions.AsyncDecorationHandler"/>
        ///   decorator and returns <c>this</c>.
        /// </summary>
        public TraceHttpResponseOptions WithDecorator(Func<StringBuilder, Task<StringBuilder>> decorator)
            => WithDecorator<TraceHttpResponseOptions>(decorator);
        
        #endregion // Fluent API    
        
        TraceHttpResponseOptions(string? messageId = null)
        : base(messageId)
        {
        }
    }
}