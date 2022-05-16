using mobileClient.ViewModels;
using TetraPak.XP.DependencyInjection;
using Xamarin.Forms.Xaml;

namespace mobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class AuthCodePage
    {
        public AuthCodePage()
        {
            InitializeComponent();
            BindingContext = XpServices.GetRequired<AuthCodeVM>();
        }
    }
}