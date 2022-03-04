#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Converters
{
    public class DirectItemUserVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is InstaDirectInboxItem data && data != null)
                return data.UserId == Helper.InstaApi.GetLoggedUser().LoggedInUser.Pk
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}