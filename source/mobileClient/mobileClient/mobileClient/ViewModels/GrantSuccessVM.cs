using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using mobileClient.Views;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Identity;
using Xamarin.Forms;

namespace mobileClient.ViewModels
{
    public class GrantSuccessVM : BaseViewModel
    {
        readonly Outcome<Grant> _outcome;

        public IEnumerable<TokenInfo> Tokens => _outcome.Value!.Tokens!;

        public ICommand UserInformationCommand { get; } 

        async Task onNavigateUserInformation()
        {
            var userInformationSvc = XpServices.GetRequired<IUserInformationService>();
            var vm = new UserInformationVM(userInformationSvc, _outcome.Value!);
            var page = new UserInformationPage(vm);
            await this.PushAsync(page);
        }
        
        public GrantSuccessVM(Outcome<Grant> outcome)
        {
            _outcome = outcome;
            // ReSharper disable once AsyncVoidLambda
            UserInformationCommand = new Command(async () => await onNavigateUserInformation());
        }
    }
}