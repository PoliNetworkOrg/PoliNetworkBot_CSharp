#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    internal class AddHashtagToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is string data)
                if (data != null && !string.IsNullOrEmpty(data))
                {
                    if (data.StartsWith("#"))
                        return data;
                    return $"#{data}";
                }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}