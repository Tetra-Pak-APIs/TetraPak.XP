using authClient.viewModels;
using Xamarin.Forms.Xaml;

namespace authClient.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserInfoPage
    {
        public UserInfoPage()
        {
            InitializeComponent();
        }

        public UserInfoPage(UserInfoVM vm) : this()
        {
            BindingContext = vm;
        }
    }
}