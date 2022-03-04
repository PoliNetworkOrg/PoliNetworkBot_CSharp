#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    internal class FollowHashtagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "Follow";
            if (value is int i)
                value = System.Convert.ToBoolean(i);

            if (value is bool data)
            {
                if (data)
                    return "Unfollow";
                return "Follow";
            }

            return "Follow";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}