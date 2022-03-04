#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    public class ViewersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is int data)
                return $"{data.ToDigits()} views";

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}