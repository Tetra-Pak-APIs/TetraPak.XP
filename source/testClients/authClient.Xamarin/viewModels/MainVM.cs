using System;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace authClient.viewModels
{
    public sealed class MainViewModel : ViewModel
    {
        public AuthCodeGrantVM? AuthCodeGrant { get; }

        public MainViewModel(IServiceProvider services, ILog? log, AuthCodeGrantVM authCodeGrant) 
        : base(services, log)
        {
            AuthCodeGrant = authCodeGrant;
        }

        // for design purposes only
        public MainViewModel() : base(null, null)
        {
        }
    }
}