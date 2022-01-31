using System;
using System.Windows.Input;
using TetraPak.XP.Logging;

namespace authClient.viewModels
{
    public class TokenVM : ViewModel
    {
        string _tokenCaption;
        string _tokenValue;
        ICommand _command;
        string _commandIcon;
        string _commandCaption;
        bool _isTokenValid;
        private bool _isTokenUnvalidated;

        public string TokenCaption
        {
            get => _tokenCaption;
            set => SetValue(ref _tokenCaption, value);
        }

        public string TokenValue
        {
            get => _tokenValue;
            set => SetValue(ref _tokenValue, value);
        }

        public bool IsTokenValid
        {
            get => _isTokenValid;
            set
            {
                SetValue(ref _isTokenValid, value);
                IsTokenUnvalidated = false;
            }
        }

        public bool IsTokenUnvalidated
        {
            get => _isTokenUnvalidated;
            set => SetValue(ref _isTokenUnvalidated, value);
        }

        public string CommandIcon
        {
            get => _commandIcon;
            set
            {
                SetValue(ref _commandIcon, value);
                OnPropertyChanged(nameof(IsCommandVisible));
            }
        }

        public string CommandCaption
        {
            get => _commandCaption;
            set
            {
                SetValue(ref _commandCaption, value);
                OnPropertyChanged(nameof(IsCommandVisible));
            }
        }
        
        public ICommand Command
        {
            get => _command;
            set
            {
                SetValue(ref _command, value);
                OnPropertyChanged(nameof(IsCommandVisible));
            }
        }

        public bool IsCommandVisible
        {
            get
            {
                var nisse = Command != null && !string.IsNullOrEmpty(_commandIcon);
                return nisse;
            }
        }

        public TokenVM() : base(null, null)
        {
        }

        public TokenVM(IServiceProvider services, ILog? log) 
        : base(services, log)
        {
            IsTokenUnvalidated = true;
        }
    }
}
