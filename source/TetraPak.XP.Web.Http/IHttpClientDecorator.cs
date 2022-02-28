using System.Net.Http;
using System.Threading.Tasks;

namespace TetraPak.XP.Web.Http
{
    /// <summary>
    ///   Classes implementing this interface can be registered to decorate <see cref="HttpClient"/>s
    ///   produced by a <see cref="IHttpClientProvider"/> (based on <see cref="TetraPakHttpClientProvider"/>). 
    /// </summary>
    public interface IHttpClientDecorator
    {
        /// <summary>
        ///   Called by a <see cref="TetraPakHttpClientProvider"/> to decorate a <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="client">
        ///   The <see cref="HttpClient"/> to be decorated.
        /// </param>
        Task<Outcome<HttpClient>> DecorateAsync(HttpClient client);
    }
}