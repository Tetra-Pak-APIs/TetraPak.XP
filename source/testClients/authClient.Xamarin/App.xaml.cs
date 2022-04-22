using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using authClient.dependencies;
using authClient.viewModels;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace authClient
{
    public partial class App : Application
    {
        readonly IEnumerable<ILogEntry> _log = new List<ILogEntry>();

        public ILog Log => IServiceProviderExtensions.GetService<ILog>(Services); 

        public IServiceProvider Services { get; }

        public static INavigation? Navigation {  get; private set; }

        public App()
        {
            var c = new XpServiceCollection();
            c.AddSingleton(p => Navigation);
            Services = this.SetupDependencies(c);
            Log.Logged += onLogged;
            Log.QueryAsync = onQueryLogAsync;
            InitializeComponent();
            try
            {
                var navigationPage = new NavigationPage(new MainPage {BindingContext = IServiceProviderExtensions.GetService<MainViewModel>(Services)});
                MainPage = navigationPage;
                Navigation = navigationPage.Navigation;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        Task<IEnumerable<ILogEntry>> onQueryLogAsync(LogRank[] ranks)
        {
            if (ranks.Length == 0)
                return Task.FromResult(_log);

            return Task.FromResult(ranks.Length == 1 
                ? _log.Where(i => i.Rank == ranks[0]) 
                : _log.Where(i => ranks.Any(e => e == i.Rank)));
        }

        void onLogged(object sender, LogEventArgs e)
        {
            ((List<ILogEntry>)_log).Add(new LogEntry(e.Rank, DateTime.Now, e.Message!));
            Debug.WriteLine(e.Rank == LogRank.Error
                ? $"[{e.Rank}] {e.Exception}{(e.Message != null ? $" {e.Message}" : "")}"
                : $"[{e.Rank}] {e.Message}");
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        class LogEntry : ILogEntry
        {
            public LogRank Rank { get; }
            public DateTime Time { get; }
            public string Message { get; }

            public LogEntry(LogRank rank, DateTime time, string message)
            {
                Rank = rank;
                Time = time;
                Message = message;
            }
        }
    }
}