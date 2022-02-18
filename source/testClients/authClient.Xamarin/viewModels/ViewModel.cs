using System;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System.Linq;
using System.Reflection;
using TetraPak.XP;
using TetraPak.XP.Logging;

namespace authClient.viewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        protected ILog? Log { get; }
        
        string _name;
        public event PropertyChangedEventHandler PropertyChanged;

        public INavigation? Navigation => Services.GetService<INavigation>() ?? null;
        public IServiceProvider Services { get; }

        /// <summary>
        ///   Useful for page/view/region captions etc.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null, params string[] otherPropertyNames)
        {
            field = value;
            OnPropertyChanged(propertyName);
            foreach (var otherPropertyName in otherPropertyNames)
            {
                OnPropertyChanged(otherPropertyName);
            }
        }

        #region .  Logging  .

        protected void LogDebug(string message) => Log?.Debug(message);
        protected void LogInfo(string message) => Log?.Information(message);
        protected void LogError(Exception exception, string message = null) => Log?.Error(exception, message);

        #endregion

        protected ViewModel(IServiceProvider services, ILog? log)
        {
            Services = services;
            initializeValidatedProperties(services, log);
            Log = log;
        }

        static readonly Type[] s_validatedValueCtorSignature = { typeof(string), typeof(ViewModel), typeof(IServiceProvider), typeof(ILog) };

        void initializeValidatedProperties(IServiceProvider services, ILog? log)
        {
            var props = GetType().GetProperties().Where(pi => pi.PropertyType.IsDerivedFrom(typeof(ValidatedValueVM<>)));
            foreach (var prop in props)
            {
                var ctor = prop.PropertyType.GetConstructor(s_validatedValueCtorSignature);
                if (ctor is null)
                    throw new NotImplementedException();

                var validatedValue = (ValidatedValueVM)ctor.Invoke(new object[] { prop.Name, this, services, log });
                prop.SetValue(this, validatedValue);
                var attr = prop.GetCustomAttributes<ValidatedValueAttribute>().FirstOrDefault();
                if (attr is null)
                    continue;

                validatedValue.AssignFrom(attr);
            }
        }

        internal virtual void NotifyChildValueChanged(ViewModel viewModel, string valueName, object oldValue, object newValue)
        {
        }
    }
}