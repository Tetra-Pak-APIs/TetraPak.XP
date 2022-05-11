using System.Threading.Tasks;
using System.Windows.Input;
using mobileClient.Fonts;
using mobileClient.Views;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using Xamarin.Forms;

namespace mobileClient.ViewModels
{
    public abstract class GrantViewModel : BaseViewModel
    {
        Outcome<Grant>? _grantOutcome;

        public Outcome<Grant>? GrantOutcome
        {
            get => _grantOutcome;
            set => SetProperty(ref _grantOutcome, value,
                triggerOtherProperties: new[]
                {
                    nameof(IsOutcomeAvailable),
                    nameof(OutcomeCaption),
                    nameof(OutcomeGlyph)
                });
        }

        public string OutcomeGlyph => IsOutcomeAvailable && GrantOutcome! ? Icons.CircleCheck : Icons.CircleExclamation;

        public bool IsOutcomeAvailable => GrantOutcome is { };

        public string OutcomeCaption
        {
            get
            {
                if (!IsOutcomeAvailable)
                    return "(pending)";

                if (GrantOutcome!.WasCancelled())
                    return "Grant request cancelled";
                    
                return GrantOutcome!
                    ? "Grant was acquired"
                    : "Request failed";
            }
        }

        public ICommand AcquireTokenCommand { get; }

        public ICommand ForceAcquireTokenCommand { get; }

        public ICommand OutcomeCommand { get; }
        
        public ICommand ToolbarItemCommand { get; }

        protected abstract Task<Outcome<Grant>> OnAcquireTokenAsync(bool forced);

        protected virtual async Task OnToolbarItemPressed()
        {
            const string ClearAllCachesAction = "Clear all caches";
            const string ClearGrantCacheAction = "Clear cached grant";
            const string ClearRefreshCacheAction = "Clear cached refresh token";
            var outcome = await PresentActionsAsync(
                "Configuration", 
                new []
                {
                    ClearAllCachesAction,
                    ClearGrantCacheAction,
                    ClearRefreshCacheAction
                });
            if (!outcome)
                return;

            switch (outcome.Value!)
            {
                case ClearAllCachesAction:
                    await OnClearAllCachesAsync();
                    break;
                case ClearGrantCacheAction:
                    await OnClearGrantCacheAsync();
                    break;
                case ClearRefreshCacheAction:
                    await OnClearRefreshCacheAsync();
                    break;
            }
        }

        protected abstract Task OnClearAllCachesAsync();

        protected abstract Task OnClearGrantCacheAsync();

        protected abstract Task OnClearRefreshCacheAsync();

        async Task onOutcomeAsync()
        {
            Page page = GrantOutcome!
                ? new GrantSuccessPage(new GrantSuccessVM(GrantOutcome!))
                : new GrantFailPage(new GrantFailVM(GrantOutcome!, Title));
            await PushAsync(page);
        }

        public GrantViewModel()
        {
            // ReSharper disable AsyncVoidLambda
            AcquireTokenCommand = new Command(async () =>
            {
                IsBusy = true;
                try
                {
                    GrantOutcome = await OnAcquireTokenAsync(false);
                }
                finally
                {
                    IsBusy = false;
                }
            });
            ForceAcquireTokenCommand = new Command(async() =>
            {
                IsBusy = true;
                try
                {
                    GrantOutcome = await OnAcquireTokenAsync(true);
                }
                finally
                {
                    IsBusy = false;
                }
            });
            OutcomeCommand = new Command(async () => await onOutcomeAsync());
            ToolbarItemCommand = new Command(async () => await OnToolbarItemPressed());
            // ReSharper restore AsyncVoidLambda
        }
    }
    
}