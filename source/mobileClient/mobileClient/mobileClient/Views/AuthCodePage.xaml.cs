using mobileClient.ViewModels;
using TetraPak.XP.DependencyInjection;
using Xamarin.Forms.Xaml;

namespace mobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthCodePage
    {
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        public AuthCodePage()
        {
            InitializeComponent();
            BindingContext = XpServices.GetRequired<AuthCodeVM>();
        }
    }
}