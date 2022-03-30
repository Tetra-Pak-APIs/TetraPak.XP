using System;
using System.Threading.Tasks;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    /// <summary>
    ///   Classes implementing this contract can be used to obtain a well-known <see cref="DiscoveryDocument"/>.  
    /// </summary>
    public interface IDiscoveryDocumentProvider
    {
        /// <summary>
        ///   Obtains a <see cref="DiscoveryDocument"/>, either from a cache or from a remote (well known) service..
        /// </summary>
        /// <param name="idToken">
        ///   (optional)<br/>
        ///   An identity token. This value can be used to resolve a "well-known" endpoint for the discovery document
        ///   to be obtained from. 
        /// </param>
        /// <param name="options">
        ///   Specifies options for how to obtain the document.
        /// </param>
        ///   An <see cref="Outcome"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="DiscoveryDocument"/> or, on failure, an <see cref="Exception"/>.
        Task<Outcome<DiscoveryDocument>> GetDiscoveryDocumentAsync(IStringValue? idToken, GrantOptions? options = null);
    }
}