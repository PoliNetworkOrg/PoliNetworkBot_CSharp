#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Converters
{
    internal class MediaTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is InstaMediaType data)
                switch (data)
                {
                    case InstaMediaType.Carousel:
                        return "";
                    case InstaMediaType.Video:
                        return "";
                }

            if (value is InstaMedia data2 && data2 != null)
            {
                if (!string.IsNullOrEmpty(data2.ProductType))
                    if (data2.ProductType.ToLower().Contains("igtv") || data2.ProductType.ToLower().Contains("ig_tv"))
                        return "";

                switch (data2.MediaType)
                {
                    case InstaMediaType.Carousel:
                        return "";
                    case InstaMediaType.Video:
                        return "";
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    internal class MediaTypeToVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is InstaMediaType data)
                switch (data)
                {
                    case InstaMediaType.Carousel:
                    case InstaMediaType.Video:
                        return Visibility.Visible;
                }

            if (value is InstaMedia data2 && data2 != null)
            {
                if (!string.IsNullOrEmpty(data2.ProductType))
                    if (data2.ProductType.ToLower().Contains("igtv") || data2.ProductType.ToLower().Contains("ig_tv"))
                        return Visibility.Collapsed;

                switch (data2.MediaType)
                {
                    case InstaMediaType.Carousel:
                    case InstaMediaType.Video:
                        return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}