#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    internal class TextToMarkdownConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            var capt = value.ToString();
            if (string.IsNullOrEmpty(capt)) return "";
            var FormatedText = "";
            var hashtags = capt.GetHashtags();
            var usernames = capt.GetUsernames();
            var dc = new Dictionary<int, string>();
            foreach (var item in hashtags)
            {
                var index = capt.IndexOf(item.ToString());
                if (!dc.ContainsKey(index))
                    dc.Add(index, item.ToString());
            }

            foreach (var item in usernames)
            {
                var index = capt.IndexOf(item.ToString());
                if (!dc.ContainsKey(index))
                    dc.Add(index, item.ToString());
            }

            var Indexes = new List<int>();
            foreach (var item in dc.OrderBy(x => x.Key)) Indexes.Add(item.Key);
            var Lastindex = 0;
            foreach (var item in Indexes)
            {
                var f = capt.Substring(Lastindex, item - Lastindex);
                FormatedText += f;
                var link = dc[item];
                if (link.StartsWith("@"))
                    FormatedText += " [" + link + "](/" + link.Replace("@", "") + ") ";
                else
                    FormatedText += " [" + link + "](" + link + ") ";
                Lastindex += f.Length + link.Length;
            }

            if (Lastindex != capt.Length) FormatedText += capt.Substring(Lastindex, capt.Length - Lastindex);
            return FormatedText == "" ? capt : FormatedText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}