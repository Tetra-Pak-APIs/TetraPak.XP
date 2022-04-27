using System;
using System.Globalization;
using TetraPak.XP;
using Xamarin.Forms;

namespace mobileClient.Converters
{
    public class OutcomeBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // todo Get colors from resource dictionary
            if (!(value is Outcome outcome))
                return value;

            var color = outcome
                ? App.Current.Resources["Success"]
                : App.Current.Resources["Failure"];
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}