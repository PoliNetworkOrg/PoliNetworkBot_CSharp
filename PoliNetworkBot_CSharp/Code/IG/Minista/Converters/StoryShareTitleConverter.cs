#region

using System;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Converters
{
    internal class StoryShareTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is InstaDirectInboxItem data && data != null)
                if (data.ItemType == InstaDirectThreadItemType.StoryShare && data.StoryShare != null &&
                    data.StoryShare.Media != null)
                    return data.UserId == Helper.InstaApi.GetLoggedUser().LoggedInUser.Pk
                        ? $"You sent {data.StoryShare.Media.User.UserName}'s story"
                        : $"Sent {data.StoryShare.Media.User.UserName}'s story";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}