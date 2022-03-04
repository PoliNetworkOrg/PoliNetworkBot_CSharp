#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    internal class UsernameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is string data)
                if (!string.IsNullOrEmpty(data))
                    return data.Truncate(8);
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}