using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;

namespace mobileClient.ViewModels
{
    public class GrantFailVM : BaseViewModel
    {
        readonly Outcome<Grant> _outcome;

        public GrantFailVM(Outcome<Grant> outcome)
        {
            _outcome = outcome;
        }
    }
}