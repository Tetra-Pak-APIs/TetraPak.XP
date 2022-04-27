using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Identity;

namespace mobileClient.ViewModels
{
    public class UserInformationVM : BaseViewModel
    {
        readonly Grant _grant;
        readonly IUserInformationService _service;

        public UserInformationVM()
        {
            Title = "User Information";
        }
        
        public UserInformationVM(IUserInformationService service, Grant grant)
        : this()
        {
            _service = service;
            _grant = grant;
        }
    }
}