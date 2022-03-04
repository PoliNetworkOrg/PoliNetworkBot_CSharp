#region

using System;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Converters
{
    internal class ReelShareConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is InstaDirectInboxItem data && data != null)
                if (data.ItemType == InstaDirectThreadItemType.ReelShare && data.ReelShareMedia != null)
                {
                    if (data.ReelShareMedia.ReelType == "reaction")
                        return data.UserId == Helper.InstaApi.GetLoggedUser().LoggedInUser.Pk
                            ? $"Reacted to their story {data.ReelShareMedia.Text}"
                            : $"Reacted to your story {data.ReelShareMedia.Text}";
                    if (data.ReelShareMedia.ReelType == "user_reel")
                        return data.UserId == Helper.InstaApi.GetLoggedUser().LoggedInUser.Pk
                            ? "Replied to their story"
                            : "Replied to your story";
                }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}