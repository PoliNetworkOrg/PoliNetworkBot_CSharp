#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Converters
{
    internal class IGTVIconToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is InstaMedia data && data != null)
                if (!string.IsNullOrEmpty(data.ProductType))
                    if (data.ProductType.ToLower().Contains("igtv") || data.ProductType.ToLower().Contains("ig_tv"))
                        return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}