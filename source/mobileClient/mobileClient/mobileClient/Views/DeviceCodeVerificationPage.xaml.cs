using mobileClient.ViewModels;
using Xamarin.Forms.Xaml;

namespace mobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class DeviceCodeVerificationPage
    {
        protected override void OnDisappearing()
        {
            if (BindingContext is DeviceCodeVM vm)
            {
                vm.CancelCommand.Execute(CancelButton.CommandParameter);
            }
            base.OnDisappearing();
        }

        public DeviceCodeVerificationPage()
        {
            InitializeComponent();
        }
        
        public DeviceCodeVerificationPage(DeviceCodeVM vm)
        : this()
        {
            BindingContext = vm;
        }
    }
}