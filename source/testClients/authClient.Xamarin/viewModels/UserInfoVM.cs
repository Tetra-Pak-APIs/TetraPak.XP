using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TetraPak.XP.Auth;
using TetraPak.XP.Logging;

namespace authClient.viewModels
{
    public class UserInfoVM : ViewModel
    {
        readonly Grant _grant;
        ObservableCollection<UserInformationItemVM> _items;

        public ObservableCollection<UserInformationItemVM> Items
        {
            get => _items;
            private set => SetValue(ref _items, value);
        }

        async void loadUserInfoAsync()
        {
            var result = await _grant.TryGetUserInformationAsync();
            if (!result)
                return;

            var userInformation = result.Value;
            var items = new List<UserInformationItemVM>();
            foreach (var type in userInformation.Types)
            {

                if (userInformation.TryGet<object>(type, out var value))
                {
                    items.Add(new UserInformationItemVM(type, value, Services, Log));
                }
            }
            Items = new ObservableCollection<UserInformationItemVM>(items);
        }

        public UserInfoVM() : base(null, null)
        {
        }

        public UserInfoVM(IServiceProvider services, Grant grant, ILog log) : base(services, log)
        {
            _grant = grant;
            loadUserInfoAsync();
            Name = "USER INFORMATION";
        }
    }

    public class UserInformationItemVM : ViewModel
    {
        string _type;
        string _value;

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        } 

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        string serializeValue(object value)
        {
            return value.ToString();
        }

        public UserInformationItemVM(string type, object value, IServiceProvider services, ILog log) : base(services, log)
        {
            Type = type;
            Value = serializeValue(value);
        }
    }
}