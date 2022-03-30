using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Auth.Abstractions;

class UserInfoLoader
{
    readonly TaskCompletionSource<Outcome<UserInformation>> _tcs;
    readonly ActorToken? _accessToken;
    readonly ILog? _log;
    
    void downloadAsync(Uri userInfoUri)
    {
        if (_accessToken is null)
            _tcs.SetResult(Outcome<UserInformation>.Fail(new Exception("No access token was provided")));
            
        Task.Run(async () =>
        {
            var request = (HttpWebRequest)WebRequest.Create(userInfoUri);
            request.Method = "GET";
            request.Accept = "*/*";
            request.Headers.Add($"Authorization: Bearer {_accessToken}");
    
            _log.DebugWebRequest(request, null);
    
            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream()
                                     ?? throw new Exception("Unexpected error: No response when requesting token.");

                using var r = new StreamReader(responseStream);
                var text = await r.ReadToEndAsync();
    
                _log?.DebugWebResponse(response as HttpWebResponse, text);
    
                var dictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(text)!;
                _tcs.SetResult(Outcome<UserInformation>.Success(new UserInformation(dictionary)));
            }
            catch (Exception ex)
            {
                _log?.Error(ex);
                _tcs.SetException(ex);
            }
            finally
            {
                _log?.Debug("[GET USER INFO END]");
            }
        });
    }
    
    public Task<Outcome<UserInformation>> AwaitDownloadedAsync() => _tcs.Task;
    
    public UserInfoLoader(ActorToken? accessToken, DiscoveryDocument discoDoc, ILog? log)
    {
        _accessToken = accessToken;
        _log = log;
        _tcs = new TaskCompletionSource<Outcome<UserInformation>>();
        downloadAsync(new Uri(discoDoc.UserInformationEndpoint!));
    }
}