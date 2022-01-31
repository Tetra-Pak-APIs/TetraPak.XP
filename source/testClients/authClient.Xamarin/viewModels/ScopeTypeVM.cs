using System;
using TetraPak.XP.Logging;

namespace authClient.viewModels
{
    public class ScopeTypeVM : ViewModel
    {
        bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        } 
        
        public ScopeTypeVM(IServiceProvider services, ILog? log) 
        : base(services, log)
        {
        }
    }
}