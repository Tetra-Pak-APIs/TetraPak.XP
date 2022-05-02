using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;

namespace mobileClient.ViewModels
{
    public class GrantFailVM : BaseViewModel
    {
        readonly Outcome<Grant>? _outcome;

        public string? Message => _outcome?.Message;
        
        public GrantFailVM()
        {
        }
        
        public GrantFailVM(Outcome<Grant> outcome, string title)
        {
            _outcome = outcome;
            Title = title;
        }
    }
}