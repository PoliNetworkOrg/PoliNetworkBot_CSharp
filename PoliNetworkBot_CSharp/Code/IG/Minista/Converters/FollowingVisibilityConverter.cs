#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Converters
{
    public class FollowingVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is InstaFriendshipShortStatus data && data != null)
                return Visibility.Visible;
            if (value is InstaUserShortFriendship data2 && data2 != null)
                return Visibility.Visible;
            if (value is InstaFriendshipStatus data3 && data3 != null)
                return Visibility.Visible;
            if (value is InstaStoryFriendshipStatus data4 && data4 != null)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}