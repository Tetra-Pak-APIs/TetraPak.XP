using mobileClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mobileClient.Views
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class GrantFailPage : ContentPage
    {
        public GrantFailPage(GrantFailVM vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}