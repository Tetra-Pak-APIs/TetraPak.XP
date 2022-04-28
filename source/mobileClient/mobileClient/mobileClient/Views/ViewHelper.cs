using System.Collections.Generic;
using Xamarin.Forms;

namespace mobileClient.Views
{
    public static class ViewHelper
    {
        public static void Bind(this Grid grid, IDictionary<string, object> dictionary, ref int row)
        {
            foreach (var pair in dictionary)
            {
                grid.RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto} );

                // claim type ...
                var label = new Label
                {
                    Text = pair.Key,
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(0d)
                };
                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);
                
                // claim value ...
                label = new Label
                {
                    Text = pair.Value.ToString(),
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
                };
                Grid.SetRow(label, row++);
                Grid.SetColumn(label, 1);
                grid.Children.Add(label);
            }
        }
    }
}