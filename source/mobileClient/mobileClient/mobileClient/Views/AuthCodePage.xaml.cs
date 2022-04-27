using mobileClient.ViewModels;
using TetraPak.XP.DependencyInjection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthCodePage : ContentPage
    {
        public AuthCodePage()
        {
            InitializeComponent();
            BindingContext = XpServices.GetRequired<AuthCodeVM>();
        }
    }
}