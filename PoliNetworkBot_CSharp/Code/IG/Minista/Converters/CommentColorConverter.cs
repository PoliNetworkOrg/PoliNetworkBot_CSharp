#region

using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

#endregion

namespace Minista.Converters
{
    public class CommentColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new SolidColorBrush(Colors.White);
            if (value is bool data && data)
                return new SolidColorBrush(Helper.GetColorFromHex("#FFB93A3A"));

            return Application.Current.Resources["DefaultForegroundColor"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}