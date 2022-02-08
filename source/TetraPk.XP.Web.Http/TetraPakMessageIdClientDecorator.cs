using System.Net.Http;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Web.Abstractions;

namespace TetraPk.XP.Web.Http
{
    public class TetraPakMessageIdClientDecorator : IHttpClientDecorator
    {
        
        
        public Task<Outcome<HttpClient>> DecorateAsync(HttpClient client)
        {
            if (client.DefaultRequestHeaders.TryGetValues(Headers.RequestMessageId, out var values))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.RequestMessageId, new RandomString());
            }
            return Task.FromResult(Outcome<HttpClient>.Success(client));
        }
    }
}