﻿using Microsoft.Extensions.Configuration;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.OAuth2;

namespace authClient.console
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class CustomAppCredentialsDelegate : IAppCredentialsDelegate
    {
        public Outcome<Credentials> GetAppCredentials(IConfiguration configuration, AuthContext context)
        {
            var prefix = context.GrantType switch
            {
                GrantType.AC => "AC-",
                GrantType.CC => "CC-",
                GrantType.DC => "DC-",
                GrantType.RF => "RF-",
                _ => string.Empty
            };
            var keyClientId = $"{prefix}{nameof(IAuthConfiguration.ClientId)}";
            var keyClientSecret = $"{prefix}{nameof(IAuthConfiguration.ClientSecret)}";

            var conf = context.Configuration;
            var clientId = conf.GetNamed<string?>(keyClientId)?.Trim();
            if (string.IsNullOrEmpty(clientId))
            {
                clientId = conf.GetNamed<string?>(nameof(IAuthConfiguration.ClientId))?.Trim();
            }

            var clientSecret = conf.GetNamed<string?>(keyClientSecret)?.Trim();
            if (string.IsNullOrEmpty(clientSecret))
            {
                clientId = conf.GetNamed<string?>(nameof(IAuthConfiguration.ClientSecret))?.Trim();
            }
            
            return string.IsNullOrEmpty(clientId)
                ? conf.MissingConfigurationOutcome<Credentials>(nameof(IAuthConfiguration.ClientId))
                : Outcome<Credentials>.Success(new Credentials(clientId, clientSecret));
        }
    }
}