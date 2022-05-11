using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TetraPak.XP;
using Xamarin.Forms;

namespace mobileClient.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        string _title = string.Empty;
        bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public double TabBarHeight => 60;
        
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null!, 
            params string[] triggerOtherProperties)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            foreach (var otherProperty in triggerOtherProperties)
            {
                OnPropertyChanged(otherProperty);
            }
            
            return true;
        }
        
        protected Task PushAsync(Page page) => ((App)Application.Current).Navigation.PushAsync(page);

        protected Task PopAsync() => ((App)Application.Current).Navigation.PopAsync();

        protected async Task<Outcome<string>> PresentActionsAsync(string title, string[] actions, string? destruction = null)
        {
            var app = (App)Application.Current;
            var navStack = app.Navigation.NavigationStack;
            destruction = string.IsNullOrEmpty(destruction) ? "Cancel" : destruction;
            var page = navStack[navStack.Count - 1] ?? app.MainPage;
            var result = await page.DisplayActionSheet(title, "Cancel", destruction, actions);
            return result == destruction
                ? Outcome<string>.Cancel()
                : Outcome<string>.Success(result);
        }

        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
