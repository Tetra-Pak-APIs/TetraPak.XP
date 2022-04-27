using mobileClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GrantSuccessPage : ContentPage
    {
        public GrantSuccessPage(GrantSuccessVM vm)
        {
            BindingContext = vm;
            InitializeComponent();
        }
    }
}