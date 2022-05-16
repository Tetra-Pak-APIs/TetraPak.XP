using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.OAuth2.AuthCode;

namespace mobileClient.ViewModels
{
    public sealed class AuthCodeVM : GrantViewModel
    {
        readonly IAuthorizationCodeGrantService _grantService;

        public string Introduction =>
            "The Authorization Code grant type is the most commonly used when a human "+
            "user makes request through a client, such as a mobile, desktop or web app.\n\n"+
            "This grant flow provides a very high level of user integrity as the client is "+
            "never allowed to access the user's secret credentials (user name and password).\n\n"+
            "Go ahead and authorize with Tetra Pak using either the 'silent' or 'forced' option. "+
            "The 'silent' option will first look a cached grant and, failing that, try and refresh "+
            "the grant using a refresh token (if available). This flow provides a 'silent' user "+
            "experience in how it avoids forcing the user to authenticate via a web browser. "+
            "The 'forced' grant option can be used to force a new grant request. "+
            "The SDK considers the 'silent' option its default but the forced option is always "+
            "available if you need it.";

        protected override async Task<Outcome<Grant>> OnAcquireTokenAsync(bool forced)
        {
            var options = forced ? GrantOptions.Forced() : GrantOptions.Default();
            return await _grantService.AcquireTokenAsync(options);
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

        public AuthCodeVM()
        {
            _grantService = null!;
            Title = "Auth Code Grant";
        }
        
        public AuthCodeVM(IAuthorizationCodeGrantService grantService)
        : this()
        {
            _grantService = grantService;
        }
    }
}