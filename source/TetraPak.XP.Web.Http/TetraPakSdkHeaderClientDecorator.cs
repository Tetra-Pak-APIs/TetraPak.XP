using System.Net.Http;
using System.Threading.Tasks;

namespace TetraPak.XP.Web.Http
{
    sealed class TetraPakSdkHeaderClientDecorator : IHttpClientDecorator
    {
        public const string TetraPakSdkHeaderIdent = "x-tetrapak-client";

        /// <inheritdoc />
        public async Task<Outcome<HttpClient>> DecorateAsync(HttpClient client)
        {
            var outcome = await client.GetTetraPakSdkRequestHeaderValueAsync();
            if (outcome)
                return Outcome<HttpClient>.Success(client);

            outcome = await client.SetTetraPakSdkRequestHeaderValueAsync();
            return outcome
                ? Outcome<HttpClient>.Success(client)
                : Outcome<HttpClient>.Fail(outcome.Exception!);
        }
    }
}