using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    public sealed class JsonWebKeySet
    {
        [JsonPropertyName("keys")]
        public IList<JsonWebKey> Keys { get; set; }

        public static async Task<Outcome<JsonWebKeySet>> DownloadAsync(string url)
        {
            using var client = new HttpClient();
            using var message = new HttpRequestMessage(HttpMethod.Get, url);
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jwk-set+json"));
            try
            {
                var response = await client.SendAsync(message);
                if (!response.IsSuccessStatusCode)
                    return Outcome<JsonWebKeySet>.Fail(
                        new HttpServerException(response, $"Error connecting to {url}: {response.ReasonPhrase}"));

                var content = await response.Content.ReadAsStringAsync();
                var jsonWebKeySet = JsonSerializer.Deserialize<JsonWebKeySet>(content);
                return Outcome<JsonWebKeySet>.Success(jsonWebKeySet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}