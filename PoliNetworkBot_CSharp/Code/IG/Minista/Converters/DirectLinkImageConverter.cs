#region

using System;
using System.Net;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Converters
{
    internal class DirectLinkImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is InstaDirectInboxItem data && data != null &&
                data.ItemType == InstaDirectThreadItemType.Link && data.LinkMedia != null &&
                data.LinkMedia?.LinkContext?.LinkImageUrl != null)
            {
                var url = data.LinkMedia.LinkContext.LinkImageUrl;
                try
                {
                    if (!string.IsNullOrEmpty(url) && url.Contains("&url="))
                    {
                        var n = url.Substring(url.IndexOf("&url=") + "&url=".Length);
                        n = n.Substring(0, n.IndexOf("&"));
                        n = WebUtility.HtmlDecode(n);
                        n = WebUtility.UrlDecode(n);
                        n = Uri.UnescapeDataString(n);
                        return new Uri(n);
                    }

                    return new Uri(url);
                }
                catch
                {
                    return new Uri(url);
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}