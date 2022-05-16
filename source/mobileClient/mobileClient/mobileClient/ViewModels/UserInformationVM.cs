using System.Collections.Generic;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Identity;
using UserInformation = TetraPak.XP.Identity.UserInformation;

namespace mobileClient.ViewModels
{
    public sealed class UserInformationVM : BaseViewModel
    {
        readonly Grant _grant;
        readonly IUserInformationService _service;

        public UserInformationVM()
        {
            Title = "User Information";
            _grant = null!;
            _service = null!;
        }

        internal async Task<Outcome<IDictionary<string,object>>> GetUserInformationAsync()
        {
            IsBusy = true;
            try
            {
                var outcome = await _service.GetUserInformationAsync(_grant, GrantOptions.Default());
                return outcome
                    ? buildDictionary(outcome.Value!)
                    : Outcome<IDictionary<string, object>>.Fail(outcome.Exception!);
            }
            finally
            {
                IsBusy = false;
            }
        }

        Outcome<IDictionary<string,object>> buildDictionary(UserInformation userInformation)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var key in userInformation.Keys)
            {
                if (!userInformation.TryGet<object>(key, out var value))
                    continue;
                    
                dictionary.Add(key, value!);
            }

            return Outcome<IDictionary<string,object>>.Success(dictionary);
        }

        public UserInformationVM(IUserInformationService service, Grant grant)
        : this()
        {
            _service = service;
            _grant = grant;
        }
    }
}