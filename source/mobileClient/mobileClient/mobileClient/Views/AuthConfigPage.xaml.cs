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
    public partial class AuthConfigPage : ContentPage
    {
        public AuthConfigPage()
        {
            InitializeComponent();
        }

        public AuthConfigPage(GrantViewModel vm)
        : this()
        {
            BindingContext = vm;
        }
    }
}