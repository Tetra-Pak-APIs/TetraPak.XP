using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TetraPak.AspNet.Debugging
{
    public abstract class AbstractTraceHttpMessageOptions
    {
        internal const int DefaultBuffersSize = 1024;
        const int DefaultMaxSizeFactor = 4;

        /// <summary>
        ///   The default value for <see cref="ForceTraceBody"/>.
        /// </summary>
        public static bool DefaultForceTraceBody { get; set; } = false;

        int _bufferSize;
        long _maxSize;

        /// <summary>
        ///   (optional)<br/>
        ///   Gets or sets a request initiator (eg. "actor").
        /// </summary>
        public string? Initiator { get; set; }
        
        /// <summary>
        ///   A base address used for the traced request message. This should be passed if
        ///   the request message's URI (<see cref="HttpRequestMessage.RequestUri"/>) is a relative path.
        ///   If <c>null</c> the request message's URI is expected to be an absolute URI (which may throw an
        ///   exception).
        /// </summary>
        public Uri? BaseAddress { get; set; }
        
        /// <summary>
        ///   Gets or sets a collection of default request headers to be passed, unless overridden
        ///   by <see cref="HttpRequestMessage.Headers"/>.
        /// </summary>
        public HttpRequestHeaders? DefaultHeaders { get; set; }
        
        /// <summary>
        ///   (optional; default=<see cref="HttpDirection.Unknown"/>)<br/>
        ///   Gets or sets 
        /// </summary>
        public HttpDirection Direction { get; set; } = HttpDirection.Unknown;

        /// <summary>
        ///   Gets or sets a value that specifies whether to hide the request URI from the trace.
        /// </summary>
        public bool HideRequestUri { get; set; }
        
        /// <summary>
        ///   Gets or sets a <see cref="string"/> to be used as a "detail" in textual representations of the traffic.
        /// </summary>
        public string? Detail { get; set; }

        /// <summary>
        ///   (static property)<br/>
        ///   Gets or sets a default value used for the <see cref="ContentLengthAsyncThreshold"/>
        ///   property's initial value.
        /// </summary>
        public static uint DefaultContentLengthAsyncThreshold { get; set; } = 1024;
        
        /// <summary>
        ///   Assign this to construct custom body content (default = <c>null</c>).
        /// </summary>
        public Func<Task<string>>? AsyncBodyFactory { get; set; }
        
        /// <summary>
        ///   Gets or sets a unique string value for tracking a request/response (mainly for diagnostics purposes).
        /// </summary>
        public string? MessageId { get; set; }
        
        /// <summary>
        ///   A callback handler, invoked after the request message has been serialized to
        ///   a <see cref="string"/> but before the result is propagated to the logger provider.
        ///   Use this to decorate the result with custom content.
        /// </summary>
        internal Func<StringBuilder, Task<StringBuilder>>? AsyncDecorationHandler { get; set; }
        
        /// <summary>
        ///   (default=<see cref="DefaultForceTraceBody"/>)<br/>
        ///   Gets or sets a value that forces the tracing of the request/response body
        /// </summary>
        public bool ForceTraceBody { get; set; } = DefaultForceTraceBody;

        /// <summary>
        ///   The buffer size. Set for reading large bodies.
        ///   Please note that the setter enforces minimum value 128. 
        /// </summary>
        public int BufferSize
        {
            get => _bufferSize;
            set => _bufferSize = AdjustBufferSize(value);
        }

        /// <summary>
        ///   A maximum length. Set this value to truncate when tracing very large bodies such as media or binaries.
        /// </summary>
        /// <remarks>
        ///   This value should be a equally divisible by <see cref="BufferSize"/> for efficiency.
        ///   The setter automatically rounds (<c>value</c> / <see cref="BufferSize"/>)
        ///   and multiplies with <see cref="BufferSize"/>.
        /// </remarks>
        public long MaxSize
        {
            get => _maxSize;
            set => _maxSize = AdjustMaxSize(value, _bufferSize);
        }

        /// <summary>
        ///   Gets or sets a value that is used when tracing large requests. Requests that exceeds this value
        ///   in content length will automatically be traced in a background thread to reduce the performance hit.
        /// </summary>
        public uint ContentLengthAsyncThreshold { get; set; } = DefaultContentLengthAsyncThreshold;

        internal static int AdjustBufferSize(int value) => Math.Max(128, value);

        internal static long AdjustMaxSize(long value, int bufferSize) => (long)Math.Round((decimal)value / bufferSize) * bufferSize;

        #region .  Fluent API  .

        internal T WithInitiator<T>(string? value, HttpDirection? direction) 
            where T : AbstractTraceHttpMessageOptions
        {
            Initiator = value;
            return direction is { } ? WithDirection<T>(direction.Value, (object) null!) : (T) this;
        }

        internal T WithInitiator<T>(object initiator, HttpDirection? direction)  
            where T : AbstractTraceHttpMessageOptions
            =>
                WithInitiator<T>(initiator.ToString() ?? initiator.GetType().ToString(), direction);

        internal T WithInitiator<T>(object initiator)  
            where T : AbstractTraceHttpMessageOptions
            =>
                WithInitiator<T>(initiator.ToString() ?? initiator.GetType().ToString(), null);

        internal T WithDirection<T>(HttpDirection value, string? initiator) 
            where T : AbstractTraceHttpMessageOptions
        {
            Direction = value;
            return string.IsNullOrEmpty(initiator) ? (T) this : WithInitiator<T>(initiator);
        }

        internal T WithDirection<T>(HttpDirection value, object? initiator) 
            where T : AbstractTraceHttpMessageOptions
            =>
                WithDirection<T>(value, initiator?.ToString() ?? initiator?.GetType().ToString());
        
        internal T WithDirection<T>(HttpDirection value) 
            where T : AbstractTraceHttpMessageOptions
            =>
                WithDirection<T>(value, null);

        internal T WithDetail<T>(string value) where T : AbstractTraceHttpMessageOptions
        {
            Detail = value;
            return (T) this;
        }
        
        internal T WithBaseAddress<T>(Uri baseAddress) where T : AbstractTraceHttpMessageOptions
        {
            BaseAddress = baseAddress;
            return (T) this;
        }

        internal T WithDefaultHeaders<T>(HttpRequestHeaders headers) where T : AbstractTraceHttpMessageOptions
        {
            DefaultHeaders = headers;
            return (T)this;
        }

        internal T WithAsyncBodyFactory<T>(Func<Task<string>>? factory) where T : AbstractTraceHttpMessageOptions
        {
            AsyncBodyFactory = factory;
            return (T)this;
        }
        
        internal T WithDecorator<T>(Func<StringBuilder, Task<StringBuilder>> decorator) 
            where T : AbstractTraceHttpMessageOptions
        {
            AsyncDecorationHandler = decorator;
            return (T)this;
        }
        
        #endregion // Fluent API        

        internal AbstractTraceHttpMessageOptions(string? messageId)
        {
            _bufferSize = DefaultBuffersSize;
            _maxSize = _bufferSize * DefaultMaxSizeFactor;
            MessageId = messageId;
        }
    }
}