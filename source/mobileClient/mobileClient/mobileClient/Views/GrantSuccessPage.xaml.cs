using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using mobileClient.ViewModels;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class GrantSuccessPage : ContentPage
    {
        void addTokens(IEnumerable<TokenInfo> tokens)
        {
            var row = 0;
            foreach (var tokenInfo in tokens)
            {
                bindTokenRole(tokenInfo, row++);
                bindTokenValue(tokenInfo, row++);
            }
            
            void bindTokenRole(TokenInfo tokenInfo, int gridRow)
            {
                var title = new Label
                {
                    Text = tokenInfo.Role.ToString().SplitCamelCase(),
                    FontSize = Device.GetNamedSize(NamedSize.Subtitle, typeof(Label))
                };
                if (Resources.TryGetValue("TokenRoleLabel", out var obj) && obj is Style titleStyle)
                {
                    title.Style = titleStyle;
                }
                Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetRow(title, gridRow);
                Grid.Children.Add(title);
            }
            
            void bindTokenValue(TokenInfo tokenInfo, int gridRow)
            {
                var bindableObject = bind(tokenInfo.Token);
                Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetRow(bindableObject, gridRow);
                Grid.Children.Add(bindableObject);
            }
        }

        static View bind(ActorToken token)
        {
            if (!token.IsJwt)
                return new Label
                {
                    Text = token.Identity,
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
                };

            var jwt = new JwtSecurityToken(token.Identity);
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            var row = 0;
            grid.Bind(jwt.Header, ref row);
            bindJwtSegmentSeparator(ref row);
            grid.Bind(jwt.Payload, ref row);
            
            void bindJwtSegmentSeparator(ref int gridRow)
            {
                var label = new Label
                {
                    Text = ".",
                    FontSize = Device.GetNamedSize(NamedSize.Subtitle, typeof(Label)),
                };
                Grid.SetRow(label, gridRow++);
                Grid.SetColumn(label, 1);
                grid.Children.Add(label);
            }
                
            return grid;
        }

        public GrantSuccessPage(GrantSuccessVM vm)
        {
            InitializeComponent();
            BindingContext = vm;
            addTokens(vm.Tokens);
        }
    }
}