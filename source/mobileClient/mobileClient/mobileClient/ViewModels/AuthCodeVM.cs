﻿using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.OAuth2.AuthCode;

namespace mobileClient.ViewModels
{
    public class AuthCodeVM : GrantViewModel
    {
        readonly IAuthorizationCodeGrantService _grantService;

        protected override async Task<Outcome<Grant>> OnAcquireTokenAsync(bool forced)
        {
            var options = forced ? GrantOptions.Forced() : GrantOptions.Default();
            return await _grantService.AcquireTokenAsync(options);
        }

        public AuthCodeVM()
        {
            Title = "Auth Code Grant";
        }
        
        public AuthCodeVM(IAuthorizationCodeGrantService grantService)
        : this()
        {
            _grantService = grantService;
        }
    }
}