#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    internal class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is long data)
                return $"{data.Divide()}";
            if (value is int data2)
                return $"{((long)data2).Divide()}";
            if (value is double data3)
                return $"{((long)data3).Divide()}";
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}