using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mobileClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceCodeVerificationPage : ContentPage
    {
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