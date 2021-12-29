#region

using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class Blacklist
    {
        internal static SpamType IsSpam(string text, long? groupId)
        {
            if (string.IsNullOrEmpty(text))
                return SpamType.ALL_GOOD;

            string[] words = text.Contains(" ") ? text.Split(' ') : new string[] { text };
            if (words != null && words.Length > 0)
            {
                var words2 = words.ToList().Select(x => x.Trim());

                foreach (var word in words2)
                {
                    if (CheckSpamLink(word, groupId) == SpamType.SPAM_LINK)
                        return SpamType.SPAM_LINK;
                }
            }

            return CheckNotAllowedWords(text, groupId) == SpamType.NOT_ALLOWED_WORDS
                ? SpamType.NOT_ALLOWED_WORDS
                : CheckForFormatMistakes(text, groupId);
        }

        private static SpamType CheckForFormatMistakes(string text, long? groupId)
        {
            if (groupId == null)
                return SpamType.ALL_GOOD;
            var specialGroups = new List<long> { -1001175999519, -1001495422899, -1001164044303 };
            if (specialGroups.All(group => groupId != group))
                return SpamType.ALL_GOOD;
            var textLower = text.ToLower();
            if (groupId == specialGroups[0])
                return textLower.Contains("#cerco") || textLower.Contains("#offro") ||
                       textLower.Contains("#searching") ||
                       textLower.Contains("#offering")
                    ? SpamType.ALL_GOOD
                    : SpamType.FORMAT_INCORRECT;
            if (groupId == specialGroups[1])
                return textLower.Contains("#richiesta") || textLower.Contains("#offerta")
                    ? SpamType.ALL_GOOD
                    : SpamType.FORMAT_INCORRECT;
            if (groupId == specialGroups[2])
                return textLower.Contains("#cerco") || textLower.Contains("#vendo")
                    ? SpamType.ALL_GOOD
                    : SpamType.FORMAT_INCORRECT;
            return SpamType.ALL_GOOD;
        }

        private static SpamType CheckNotAllowedWords(string text, long? groupId)
        {
            text = text.ToLower();

            var s = text.Split(' ');
            foreach (var s2 in s)
            {
                var s3 = RemoveUselessCharacters(s2);

                if (!string.IsNullOrEmpty(s3))
                    switch (s3)
                    {
                        case "porcodio":
                        case "dioporco":
                        case "diocane":
                        case "negro":
                        case "negri":
                        case "negra":
                        case "negre":
                            return SpamType.NOT_ALLOWED_WORDS;
                    }
            }

            var specialGroups = new List<long> { -1001361547847, -452591994, -1001320704409 };
            if (groupId != null && text.Contains("bitcoin") &&
                (text.Contains("guadagno") || text.Contains("rischio")) && specialGroups.All(group => groupId != group))
                return SpamType.NOT_ALLOWED_WORDS;

            return SpamType.ALL_GOOD;
        }

        private static string RemoveUselessCharacters(string s3)
        {
            if (string.IsNullOrEmpty(s3))
                return null;

            var r = "";
            foreach (var c in s3)
                if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z')
                    r += c;

            return r;
        }

        private static SpamType CheckSpamLink(string text, long? groupId)
        {
            switch (groupId)
            {
                case -1001307671408: //gruppo politica
                    {
                        return SpamType.ALL_GOOD;
                    }

                default:
                    {
                        return CheckSpamLink_DefaultGroup(text);
                    }
            }
        }

        private static SpamType CheckSpamLink_DefaultGroup(string text)
        {
            if (!text.Contains("t.me/"))
            {
                var b1 = text.Contains("facebook.com") ||
                         text.Contains("whatsapp.com") ||
                         text.Contains("instagram.com") ||
                         text.Contains("bit.ly") ||
                         text.Contains("is.gd") ||
                         text.Contains("amzn.to") ||
                         text.Contains("goo.gl") ||
                         text.Contains("forms.gle") ||
                         text.Contains("docs.google.com") ||
                         text.Contains("amazon.it/gp/student") ||
                         text.Contains("amazon.com/gp/student") ||
                         text.Contains("polinetwork.it") ||
                         text.Contains("discord.gg");
                return b1 ? SpamType.SPAM_LINK : SpamType.ALL_GOOD;
            }

            if (text.Contains("t.me/"))
            {
                if (text.Contains("t.me/c/"))
                    return SpamType.ALL_GOOD;

                var text1 = text.ToLower();
                var t2 = text1.Split("/");
                var t3 = Find(t2, "t.me");
                if (t3 != null)
                {
                    var t4 = t2[t3.Value + 1];
                    var valid = CheckIfAllowedTag(t4);
                    if (valid == SpamType.ALL_GOOD)
                        return SpamType.ALL_GOOD;
                }
            }

            var isOurLink = CheckIfIsOurTgLink(text);
            var b2 = isOurLink != null && !isOurLink.Value;
            return b2 ? SpamType.SPAM_LINK : SpamType.ALL_GOOD;
        }

        private static SpamType CheckIfAllowedTag(string t4)
        {
            if (string.IsNullOrEmpty(t4)) return SpamType.ALL_GOOD;

            if (t4.StartsWith("@")) t4 = t4[1..];

            var b = GlobalVariables.AllowedTags.Contains(t4); //this part is useless, telegram does not have links t.me/@*
            return b ? SpamType.ALL_GOOD : SpamType.SPAM_LINK;
        }

        private static long? Find(string[] t2, string v)
        {
            for (var i = 0; i < t2.Length; i++)
            {
                var t3 = t2[i];
                if (t3 == v)
                    return i;
            }

            return null;
        }

        private static bool? CheckIfIsOurTgLink(string text)
        {
            const string q1 = "SELECT id FROM Groups WHERE link = @link";
            var link = GetTelegramLink(text);

            if (string.IsNullOrEmpty(link))
                return null;

            link = link.Trim();

            var dt = SqLite.ExecuteSelect(q1, new Dictionary<string, object> { { "@link", link } });
            var value = SqLite.GetFirstValueFromDataTable(dt);
            if (value == null)
                return false;
            var s = value.ToString();
            return !string.IsNullOrEmpty(s);
        }

        private static string GetTelegramLink(string text)
        {
            var s = text.Contains(" ") ? text.Split(' ') : new[] { text };
            return s.FirstOrDefault(s2 => s2.ToLower().Contains("t.me/"));
        }

        internal static SpamType IsSpam(PhotoSize[] photo)
        {
            var biggerphoto = UtilsPhoto.GetLargest(photo);
            if (biggerphoto == null)
                return SpamType.ALL_GOOD;

            //todo: analizzare la foto con un ocr

            return SpamType.ALL_GOOD;
        }
    }
}