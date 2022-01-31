using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using authClient.viewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace authClient.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogPage : ContentPage
    {
        public LogPage()
        {
            InitializeComponent();
        }

        internal LogPage(LogVM vm) : this()
        {
            BindingContext = vm;
        }
    }
}