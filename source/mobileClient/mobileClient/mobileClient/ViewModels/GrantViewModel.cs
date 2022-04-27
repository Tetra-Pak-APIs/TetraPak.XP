using System.Threading.Tasks;
using System.Windows.Input;
using mobileClient.Views;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using Xamarin.Forms;

namespace mobileClient.ViewModels
{
    public abstract class GrantViewModel : BaseViewModel
    {
        static INavigation Navigation => ((App)Application.Current).Navigation;
        
        Outcome<Grant>? _grantOutcome;

        public Outcome<Grant>? GrantOutcome
        {
            get => _grantOutcome;
            set => SetProperty(ref _grantOutcome, value,
                triggerOtherProperties: new[]
                {
                    nameof(IsOutcomeAvailable),
                    nameof(OutcomeCaption)
                });
        }

        public bool IsOutcomeAvailable => GrantOutcome is { };

        public string OutcomeCaption => IsOutcomeAvailable && GrantOutcome!
            ? "Grant was acquired"
            : "Grant was denied";

        public ICommand AcquireTokenCommand { get; }

        public ICommand ForceAcquireTokenCommand { get; }

        public ICommand OutcomeCommand { get; }

        protected Task PushAsync(Page page) => Navigation.PushAsync(page);

        protected abstract Task<Outcome<Grant>> OnAcquireTokenAsync(bool forced);
        
        async Task onOutcomeAsync()
        {
            Page page = GrantOutcome!
                ? new GrantSuccessPage(new GrantSuccessVM(GrantOutcome!))
                : new GrantFailPage(new GrantFailVM(GrantOutcome!));
            await PushAsync(page);
        }

        public GrantViewModel()
        {
            // ReSharper disable AsyncVoidLambda
            AcquireTokenCommand = new Command(async () => GrantOutcome = await OnAcquireTokenAsync(false));
            ForceAcquireTokenCommand = new Command(async() => GrantOutcome = await OnAcquireTokenAsync(true));
            OutcomeCommand = new Command(async () => await onOutcomeAsync());
            // ReSharper restore AsyncVoidLambda
        }
    }
}