#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    internal class BooleanReverseToVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Visible;
            if (value is bool data)
                return !data ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}