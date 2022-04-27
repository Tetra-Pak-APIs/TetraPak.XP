using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.OAuth2.ClientCredentials;

namespace mobileClient.ViewModels
{
    public class ClientCredentialsVM : GrantViewModel
    {
        readonly IClientCredentialsGrantService _grantService;

        protected override async Task<Outcome<Grant>> OnAcquireTokenAsync(bool forced)
        {
            var options = forced ? GrantOptions.Forced() : GrantOptions.Default();
            return await _grantService.AcquireTokenAsync(options);
        }

        public ClientCredentialsVM()
        {
            Title = "Client Credentials Grant";
        }
        
        public ClientCredentialsVM(IClientCredentialsGrantService grantService)
        : this()
        {
            _grantService = grantService;
        }
    }
}