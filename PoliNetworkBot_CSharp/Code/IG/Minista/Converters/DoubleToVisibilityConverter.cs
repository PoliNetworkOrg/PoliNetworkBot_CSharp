#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    internal class DoubleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is double data)
            {
                if (data == 0)
                    return Visibility.Collapsed;
                if (data > 0 && data <= 100)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }

            if (value is int data2)
            {
                if (data2 > 0)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}