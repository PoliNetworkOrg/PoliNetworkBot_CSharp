#region

using System;
using System.Linq;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Converters
{
    internal class CarouselImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is InstaCarousel data)
                if (data?.Count > 0)
                    try
                    {
                        return new Uri(data.FirstOrDefault().Images.FirstOrDefault().Uri);
                    }
                    catch
                    {
                    }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}