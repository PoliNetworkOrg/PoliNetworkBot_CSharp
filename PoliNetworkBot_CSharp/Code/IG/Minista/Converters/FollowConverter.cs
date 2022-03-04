#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    public class FollowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is long data)
                return $"|  {data.Divide()} followers";

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}