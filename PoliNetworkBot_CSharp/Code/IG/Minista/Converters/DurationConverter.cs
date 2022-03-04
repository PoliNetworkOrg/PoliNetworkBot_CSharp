#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    public class DurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is double data)
            {
                if (data == 0)
                    return "";

                var span = TimeSpan.FromSeconds(data);

                return $"{span.Minutes}:{span.Seconds}";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}