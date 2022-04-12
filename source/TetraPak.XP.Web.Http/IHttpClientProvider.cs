using System;
using System.Net.Http;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Web.Http
{
    /// <summary>
    ///   Classes implementing this interface can be used as a factory for obtaining <see cref="HttpClient"/>s.
    /// </summary>
    public interface IHttpClientProvider // todo Rewriting to support newer AuthContext concept
    {
        /// <summary>
        ///   Creates and returns a (configured) <see cref="HttpClient"/> for use with a specific service. 
        /// </summary>
        /// <param name="options">
        ///   (optional)<br/>
        ///   A (customizable) set of options to describe the requested <see cref="HttpClient"/>.
        /// </param>
        /// <param name="authContext">
        ///   (optional)<br/>
        ///   An authorization context.
        ///   Passing this object allows the provider to automatically authorize the client
        ///   for the intended remote service consumption.  
        /// </param>
        ///   (optional)<br/>
        ///   An authorization context, for use in      
        /// <returns>
        ///   A <see cref="Outcome{T}"/> value indicating success/failure and, on success, carrying
        ///   the requested client as its <see cref="Outcome{T}.Value"/>; otherwise an <see cref="Exception"/>.
        /// </returns>
        Task<Outcome<HttpClient>> GetHttpClientAsync(
            HttpClientOptions? options = null,
            AuthContext? authContext = null);
    }
}