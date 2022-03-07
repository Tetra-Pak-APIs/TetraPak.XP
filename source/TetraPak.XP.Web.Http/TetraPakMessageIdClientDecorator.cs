using System.Net.Http;
using System.Threading.Tasks;
using TetraPak.XP.Web.Abstractions;

namespace TetraPak.XP.Web.Http
{
    public class TetraPakMessageIdClientDecorator : IHttpClientDecorator
    {
        public Task<Outcome<HttpClient>> DecorateAsync(HttpClient client)
        {
            if (!client.DefaultRequestHeaders.TryGetValues(Headers.MessageId, out _))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.MessageId, new RandomString());
            }
            return Task.FromResult(Outcome<HttpClient>.Success(client));
        }
    }
}