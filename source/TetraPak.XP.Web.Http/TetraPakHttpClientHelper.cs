using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TetraPak.XP.ApplicationInformation;

namespace TetraPak.XP.Web.Http
{
    public static class TetraPakHttpClientHelper
    {
        public static Task<Outcome<IEnumerable<string>>> GetTetraPakSdkRequestHeaderValueAsync(this HttpClient client)
        {
            return Task.FromResult(client.DefaultRequestHeaders.TryGetValues(
                TetraPakSdkHeaderClientDecorator.TetraPakSdkHeaderIdent,
                out var values) ? Outcome<IEnumerable<string>>.Success(values) : Outcome<IEnumerable<string>>.Fail("Header not found"));
        }

        public static Task<Outcome<IEnumerable<string>>> SetTetraPakSdkRequestHeaderValueAsync(this HttpClient client)
        {
            if (ApplicationInfo.Current is null)
                return Task.FromResult(Outcome<IEnumerable<string>>.Fail("SDK info is missing"));

            try
            {
                client.DefaultRequestHeaders.Add(TetraPakSdkHeaderClientDecorator.TetraPakSdkHeaderIdent, ApplicationInfo.Current.StringValue);
                return Task.FromResult(Outcome<IEnumerable<string>>.Success(Array.Empty<string>()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}