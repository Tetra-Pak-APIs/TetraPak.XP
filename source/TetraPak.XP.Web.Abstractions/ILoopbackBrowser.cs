using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TetraPak.XP.Web.Abstractions
{
    /// <summary>
    ///   Classes implementing this contract can be used by apps to open a browser,
    ///   send requests and then wait for loopback requests back to a specified URI.
    /// </summary>
    public interface ILoopbackBrowser : IDisposable
    {
        /// <summary>
        ///   HTML content to be sent to the loopback browser after the request/response roundtrip is complete.
        /// </summary>
        string? HtmlResponse { get; set; }
        
        /// <summary>
        ///   Sends a request to the browser and returns a request to a specified loopback host address.
        /// </summary>
        /// <param name="target">
        ///   The initial target <see cref="Uri"/>. 
        /// </param>
        /// <param name="loopbackHost">
        ///   The expected loopback host address.
        /// </param>
        /// <param name="filter">
        ///   (optional)<br/>
        ///   Specifies a custom requisite for accepting a loopback request.
        ///   The default filter will accept any HTTP 'GET' request.  
        /// </param>
        /// <param name="cancellationToken">
        ///   (optional)<br/>
        ///   Enables cancellation of the operation.
        /// </param>
        /// <param name="timeout">
        ///   (optional)<br/>
        ///   Specifies a timeout for the operation, after which it will end (and return a failed outcome). 
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="HttpRequest"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        Task<Outcome<GenericHttpRequest>> GetLoopbackAsync(
            Uri target, 
            Uri loopbackHost, 
            LoopbackFilter? filter = null,
            CancellationToken? cancellationToken = null,
            TimeSpan? timeout = null);
    }
}