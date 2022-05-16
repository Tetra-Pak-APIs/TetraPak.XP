using System.Collections.Generic;
using System.Threading.Tasks;
using mobileClient.ViewModels;
using TetraPak.XP;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class UserInformationPage : ContentPage
    {
        async Task populateFrom(UserInformationVM vm)
        {
            var outcome = await vm.GetUserInformationAsync();
            if (outcome)
            {
                MainThread.BeginInvokeOnMainThread(() => populateInformation(outcome.Value!));
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => populateFailure(outcome));
            }
        }

        void populateInformation(IDictionary<string, object> dictionary)
        {
            var row = 0;
            Grid.Bind(dictionary, ref row);
        }
            
        void populateFailure(Outcome<IDictionary<string,object>> outcome)
        {
            // todo
        }
        
        public UserInformationPage(UserInformationVM vm)
        {
            InitializeComponent();
            BindingContext = vm;
            Task.Run(() => populateFrom(vm));
        }

    }
}