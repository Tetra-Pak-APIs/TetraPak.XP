using System;
using System.Collections.Generic;

namespace TetraPak.XP.Auth.Abstractions
{

    public static class GrantHelper
    {
        internal static Grant ForAuthCode(this Grant self,
            ActorToken accessToken,
            DateTime expires,
            ActorToken? refreshToken,
            ActorToken? idToken)
        {
            var tokens = new List<TokenInfo>(new[]
            {
                new TokenInfo(accessToken, TokenRole.AccessToken, expires)
            });
            if (refreshToken is { })
            {
                tokens.Add(new TokenInfo(refreshToken, TokenRole.RefreshToken));
            }

            if (idToken is { })
            {
                tokens.Add(new TokenInfo(idToken, TokenRole.IdToken));
            }

            return new Grant(tokens.ToArray());
        }

        internal static Grant ForClientCredentials(this Grant self, ActorToken accessToken, DateTime expires)
        {
            return new Grant(new TokenInfo(accessToken, TokenRole.AccessToken, expires));
        }

        // internal static Grant ForDeviceCode(this Grant self, obsolete
        //     ActorToken accessToken, 
        //     DateTime expires,
        //     ActorToken? refreshToken, 
        //     ActorToken? idToken)
        // {
        //     var tokens = new List<TokenInfo>(new []
        //     {
        //         new TokenInfo(accessToken, TokenRole.AccessToken, expires)
        //     });
        //     if (refreshToken is { })
        //     {
        //         tokens.Add(new TokenInfo(refreshToken, TokenRole.RefreshToken));
        //     }
        //     if (idToken is {})
        //     {
        //         tokens.Add(new TokenInfo(idToken, TokenRole.IdToken));
        //     }
        //     return new Grant(tokens.ToArray());
        // }

        // /// <summary>
        // ///   Attempts obtaining user information.
        // /// </summary>
        // /// <returns></returns>
        // /// <remarks>
        // /// </remarks>
        // public static async Task<Outcome<UserInformation>> TryGetUserInformationAsync(
        //     this Grant grant,
        //     ITimeLimitedRepositories? cache, 
        //     ILog? log = null)
        // {
        //     const string KeyUserInfo = "__userInfo"; 
        //     
        //     log?.Debug("[GET USER INFORMATION BEGIN]");
        //
        //     if (grant.AccessToken is null)
        //     {
        //         var error = new Exception("Cannot get user information without a valid access token");
        //         log?.Warning(error.Message);
        //         return Outcome<UserInformation>.Fail(error);
        //     }
        //
        //     var cachedUserInfo = grant.GetValue<UserInformation>(KeyUserInfo, null!);
        //     if (cachedUserInfo != null)
        //     {
        //         log?.Debug("User information was cached");
        //         return Outcome<UserInformation>.Success(cachedUserInfo);
        //     }
        //
        //     try
        //     {
        //         log?.Debug("Retrieves user information from API ...");
        //         var discoDoc = DiscoveryDocument.Current;
        //         if (discoDoc is null)
        //         {
        //             var gotDiscoDoc = await DiscoveryDocument.TryDownloadAndSetCurrentAsync(grant, cache);
        //             if (!gotDiscoDoc)
        //             {
        //                 var error = new Exception("Failed to retrieve the discovery document. Cannot resolve user information endpoint");
        //                 log?.Debug($"ERROR: {error.Message}");
        //                 return Outcome<UserInformation>.Fail(error);
        //             }
        //
        //             discoDoc = gotDiscoDoc.Value!;
        //         }
        //     
        //         var userInfoLoader = new UserInfoLoader(grant.AccessToken, discoDoc, log);
        //         var userInfoOutcome = await userInfoLoader.AwaitDownloadedAsync();
        //         if (!userInfoOutcome)
        //         {
        //             log?.Warning($"Failed when downloading user information. {userInfoOutcome.Message}");
        //             
        //         }
        //         log?.Debug("Successfully received user information from API");
        //         grant.SetValue(KeyUserInfo, userInfoOutcome);
        //         return Outcome<UserInformation>.Success(userInfoOutcome.Value!);
        //     }
        //     catch (Exception ex)
        //     {
        //         var message = $"Failed while retrieving user information from API. {ex.Message}";
        //         log?.Error(ex, message);
        //         return Outcome<UserInformation>.Fail(new Exception(message, ex));
        //     }
        //     finally
        //     {
        //         log?.Debug("[GET USER INFORMATION END]");
        //     }
        // }
    }
}