#region

using System;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Converters
{
    public class CaptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is InstaCaption data)
                if (data != null && !string.IsNullOrEmpty(data.Text))
                    return data.Text.Truncate();
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}