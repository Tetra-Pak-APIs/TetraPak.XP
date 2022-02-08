using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPk.XP.Web.Http
{
    /// <summary>
    ///   Classes implementing this interface can be used as a factory for obtaining <see cref="HttpClient"/>s.
    /// </summary>
    public interface IHttpClientProvider
    {
        /// <summary>
        ///   Creates and returns a (configured) <see cref="HttpClient"/> for use with a specific service. 
        /// </summary>
        /// <param name="options">
        ///     (optional)<br/>
        ///     A (customizable) set of options to describe the requested <see cref="HttpClient"/>.
        /// </param>
        /// <param name="cancellationToken">
        ///     (optional)<br/>
        ///     A <see cref="CancellationToken"/>
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome{T}"/> value indicating success/failure and, on success, carrying
        ///   the requested client as its <see cref="Outcome{T}.Value"/>; otherwise an <see cref="Exception"/>.
        /// </returns>
        Task<Outcome<HttpClient>> GetHttpClientAsync(
            SecureClientOptions? options = null,
            CancellationToken? cancellationToken = null);
    }
}