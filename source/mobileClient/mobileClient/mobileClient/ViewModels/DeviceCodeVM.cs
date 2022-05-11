using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using mobileClient.Views;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.OAuth2.DeviceCode;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace mobileClient.ViewModels
{
    public class DeviceCodeVM : GrantViewModel
    {
        readonly IDeviceCodeGrantService _grantService;
        string? _userCode;
        string? _verificationUri;
        string? _expires;

        public string Introduction =>
            "The Device Code grant type is typically not the preferred grant type for " +
            "mobile clients but is included for clarity and to demonstrate the flow.\n\n" +
            
            "This grant type is well suited for use cases where the you want the user " +
            "to authenticate using a different device. While this offers some added security " +
            "it might also be necessary if client being authorized cannot offer a convenient " +
            "user interface for typing in a device code, such as a keyboard.\n\n" +
            
            "The grant type flow begins with the user initiating a request for a device code grant. "+
            "The authority responds by generating a one-time device code and passes it back to the" +
            "client, along with a URL that can be used to submit the device code " +
            "(presumably on a different device). The client then repeatedly polls the token issuer " +
            "service waiting for the device code verification. When the user has verified the code " +
            "the client gets a successful response that contains the requested grant.";

        public string IntroductionUserCode => 
            "You now need to verify the below user code.\n\n"+
            
            "This should normally be done from a different device, by browsing to the URL presented "+ 
            "but for sake of demonstration you can click the code to automatically copy it to the clipboard " +
            "and open a web browser that navigates to that URL.";
        
        public string? UserCode
        {
            get => _userCode;
            set => SetProperty(ref _userCode, value);
        }

        public string? VerificationUri
        {
            get => _verificationUri;
            set => SetProperty(ref _verificationUri, value);
        }

        public string? Expires
        {
            get => _expires;
            set => SetProperty(ref _expires, value);
        }

        public ICommand CancelCommand { get; }

        public ICommand UserCodeCommand { get; }

        public ICommand UrlCommand { get; }

        protected override async Task<Outcome<Grant>> OnAcquireTokenAsync(bool forced)
        {
            var cts = new CancellationTokenSource();
            var options = forced ? GrantOptions.Forced(cts) : GrantOptions.Default(cts);
            IsBusy = true;
            Page? authPage = null; 
            try
            {
                
#pragma warning disable CS1998
                var outcome = await _grantService.AcquireTokenAsync(options, async e =>
                {
                    UserCode = e.UserCode;
                    VerificationUri = e.VerificationUri.ToString();
                    Expires = XpDateTime.Now.Add(e.ExpiresIn).ToString("U");
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        authPage = new DeviceCodeVerificationPage(this);
                        await PushAsync(authPage);
                    });
                });
                return outcome;
            }
            finally
            {
                if (authPage is { })
                {
                    await PopAsync();
                }
                IsBusy = false;
            }
#pragma warning restore CS1998
        }
        
        protected override async Task OnClearAllCachesAsync()
        {
            await OnClearGrantCacheAsync();
            await OnClearRefreshCacheAsync();
        }

        protected override async Task OnClearGrantCacheAsync()
        {
            await _grantService.ClearCachedGrantsAsync();
        }

        protected override async Task OnClearRefreshCacheAsync()
        {
            await _grantService.ClearCachedRefreshTokensAsync();
        }

        async Task onCancelAsync() => await _grantService.CancelAsync();

        async Task onUserCodeAsync() => await Clipboard.SetTextAsync(UserCode);

        async Task onUrlAsync() => await Clipboard.SetTextAsync(VerificationUri);

        public DeviceCodeVM()
        {
            Title = "Device Code Grant";
            _grantService = null!;
            
            // ReSharper disable AsyncVoidLambda
            CancelCommand = new Command(async () => await onCancelAsync());
            UserCodeCommand = new Command(async () => await onUserCodeAsync());
            UrlCommand = new Command(async () => await onUrlAsync());
            // ReSharper restore AsyncVoidLambda
        }

        public DeviceCodeVM(IDeviceCodeGrantService grantService)
        : this()
        {
            _grantService = grantService;
        }
    }
}