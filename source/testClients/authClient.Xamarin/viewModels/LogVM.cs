using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;
using Xamarin.Forms;

namespace authClient.viewModels
{
    class LogVM : ViewModel
    {
        public ObservableCollection<LogItemVM> Items { get; }

        public ICommand CopyCommand { get; }

        public LogVM() : base(null, null)
        {
        }

        internal LogVM(IServiceProvider services, ILog? log) 
        : base(services, log)
        {
            CopyCommand = new Command(copyToClipboard);
            Items = new ObservableCollection<LogItemVM>();
            refreshItems();
        }

        async void copyToClipboard()
        {
            var items = await Log.QueryAsync.Invoke();
            var log = new StringBuilder();
            foreach (var entry in items)
            {
                log.AppendLine();
                log.AppendLine($"[{entry.Rank.ToString()[0]}]");
                log.AppendLine(entry.Message);
            }

            await Xamarin.Essentials.Clipboard.SetTextAsync(log.ToString());
        }

        async void refreshItems()
        {
            if (Log.QueryAsync is null)
                return;

            Items.Clear();
            var items = await Log.QueryAsync.Invoke();
            foreach (var entry in items)
            {
                Items.Add(new LogItemVM(entry, Services, Log));
            }
        }
    }

    class LogItemVM : ViewModel
    {
        readonly ILogEvent _event;

        public Color MessageColor { get; }

        public string Message => _event.Message;

        public LogItemVM(ILogEvent logEvent, IServiceProvider services, ILog log) : base(services, log)
        {
            _event = logEvent;
            switch (logEvent.Rank)
            {
                case LogRank.Debug:
                    MessageColor = Color.Yellow;
                    break;

                case LogRank.Information:
                    MessageColor = Color.White;
                    break;

                case LogRank.Error:
                    MessageColor = Color.Crimson;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
