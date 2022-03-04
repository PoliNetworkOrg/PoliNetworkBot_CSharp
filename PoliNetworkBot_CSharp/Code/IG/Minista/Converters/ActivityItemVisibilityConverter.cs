#region

using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Converters
{
    internal class ActivityItemVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is InstaRecentActivityFeed data && data != null)
            {
                if (data.Medias?.Count > 1)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }

            if (value is List<InstaActivityMedia> data2)
                if (data2 == null || data2?.Count <= 1)
                    return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}