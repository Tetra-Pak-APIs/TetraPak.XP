using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;

namespace mobileClient.ViewModels
{
    public class GrantSuccessVM : BaseViewModel
    {
        readonly Outcome<Grant> _outcome;
        
        public GrantSuccessVM(Outcome<Grant> outcome)
        {
            _outcome = outcome;
        }
    }
}