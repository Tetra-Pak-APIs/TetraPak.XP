using System;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Web.credentialsAndTokens;

namespace TetraPak.XP.Web
{
    /// <summary>
    ///   Classes implementing this interface are able to obtain client authorization,
    ///   resulting in an <see cref="ActorToken"/> to be used in requests.   
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        ///   Authenticates a specific service. 
        /// </summary>
        /// <param name="options">
        ///   Options for obtaining a client.
        /// </param>
        /// <param name="cancellationToken">
        ///   (optional)<br/>
        ///   A <see cref="CancellationToken"/>
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome{T}"/> value indicating success/failure and, on success, carrying
        ///   the requested token as its <see cref="Outcome{T}.Value"/>; otherwise an <see cref="Exception"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   <paramref name="options"/> configuration (<see cref="HttpClientOptions.AuthConfig"/>) was unassigned.
        /// </exception>
        Task<Outcome<ActorToken>> AuthorizeAsync(HttpClientOptions options, CancellationToken? cancellationToken = null);
    }
}