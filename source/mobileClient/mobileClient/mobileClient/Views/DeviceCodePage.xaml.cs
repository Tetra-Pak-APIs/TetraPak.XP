using mobileClient.ViewModels;
using TetraPak.XP.DependencyInjection;
using Xamarin.Forms.Xaml;

namespace mobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class DeviceCodePage
    {
        public DeviceCodePage()
        {
            InitializeComponent();
            BindingContext = XpServices.GetRequired<DeviceCodeVM>();
        }
    }
}