#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    public class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is string s))
                return null;

            if (Uri.TryCreate(s, UriKind.Absolute, out var uri))
                return uri;

            if (Uri.TryCreate(s, UriKind.Relative, out uri))
                return uri;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var uri = value as Uri;
            if (uri == null)
                return null;

            return uri.OriginalString;
        }
    }
}