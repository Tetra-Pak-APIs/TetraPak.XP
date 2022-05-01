using System;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.OAuth2.DeviceCode;

namespace mobileClient.ViewModels
{
    public class DeviceCodeVM : GrantViewModel
    {
        readonly IDeviceCodeGrantService _grantService;
        string _userCode;
        string _verificationUri;
        string _expires;

        public string UserCode
        {
            get => _userCode;
            set => SetProperty(ref _userCode, value);
        }

        public string VerificationUri
        {
            get => _verificationUri;
            set => SetProperty(ref _verificationUri, value);
        }

        public string Expires
        {
            get => _expires;
            set => SetProperty(ref _expires, value);
        }

        protected override async Task<Outcome<Grant>> OnAcquireTokenAsync(bool forced)
        {
            var options = forced ? GrantOptions.Forced() : GrantOptions.Default();
            return await _grantService.AcquireTokenAsync(options, e =>
            {
                UserCode = e.UserCode;
                VerificationUri = e.VerificationUri.ToString();
                Expires = XpDateTime.Now.Add(e.ExpiresIn).ToString("U");
            });
        }

        public DeviceCodeVM()
        {
            Title = "Device Code Grant";
        }

        public DeviceCodeVM(IDeviceCodeGrantService grantService)
        : this()
        {
            _grantService = grantService;
        }

    }
}