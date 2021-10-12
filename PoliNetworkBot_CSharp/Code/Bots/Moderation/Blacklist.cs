#region

using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
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

            var isSpamLink = CheckSpamLink(text, groupId);
            return isSpamLink == SpamType.SPAM_LINK ? SpamType.SPAM_LINK : CheckNotAllowedWords(text);
        }

        private static SpamType CheckNotAllowedWords(string text)
        {
            text = text.ToLower();

            var s = text.Split(' ');
            foreach (var s2 in s)
            {
                string s3 = RemoveUselessCharacters(s2);

                if (!string.IsNullOrEmpty(s3))
                {
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
            }

            if (text.Contains("bitcoin") && (text.Contains("guadagno") || text.Contains("rischio")))
                return SpamType.NOT_ALLOWED_WORDS;

            return SpamType.ALL_GOOD;
        }

        private static string RemoveUselessCharacters(string s3)
        {
            if (string.IsNullOrEmpty(s3))
                return null;

            string r = "";
            foreach (var c in s3)
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                {
                    r += c;
                }
            }

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
                bool b1 = text.Contains("facebook.com") ||
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
                       text.Contains("discord.gg");
                return b1 ? SpamType.SPAM_LINK : SpamType.ALL_GOOD;
            }

            if (text.Contains("t.me/"))
            {
                if (text.Contains("t.me/c/"))
                    return SpamType.ALL_GOOD;

                text = text.ToLower();
                var t2 = text.Split("/");
                int? t3 = Find(t2, "t.me");
                if (t3 != null)
                {
                    var t4 = t2[t3.Value + 1];
                    var valid = CheckIfAllowedTag(t4);
                    if (valid == SpamType.ALL_GOOD)
                        return SpamType.ALL_GOOD;
                }
            }

            var isOurLink = CheckIfIsOurTgLink(text);
            bool b2 = isOurLink != null && !isOurLink.Value;
            return b2 ? SpamType.SPAM_LINK : SpamType.ALL_GOOD;
        }

        private static SpamType CheckIfAllowedTag(string t4)
        {
            if (string.IsNullOrEmpty(t4))
            {
                return SpamType.ALL_GOOD;
            }

            if (t4.StartsWith("@"))
            {
                t4 = t4[1..];
            }

            bool b = GlobalVariables.AllowedTags.Contains(t4);
            return b ? SpamType.ALL_GOOD : SpamType.SPAM_LINK;
        }

        private static int? Find(string[] t2, string v)
        {
            for (int i = 0; i < t2.Length; i++)
            {
                string t3 = t2[i];
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

            var dt = SqLite.ExecuteSelect(q1, new Dictionary<string, object> { { "@link", link } });
            var value = SqLite.GetFirstValueFromDataTable(dt);
            if (value == null)
                return false;
            var s = value.ToString();
            return !string.IsNullOrEmpty(s);
        }

        private static string GetTelegramLink(string text)
        {
            var s = text.Split(' ');
            return s.FirstOrDefault(s2 => s2.Contains("t.me/"));
        }

        internal static SpamType IsSpam(PhotoSize[] photo)
        {
            PhotoSize biggerphoto = Utils.UtilsMedia.UtilsPhoto.GetLargest(photo);
            if (biggerphoto == null)
                return SpamType.ALL_GOOD;

            //todo: analizzare la foto con un ocr

            return SpamType.ALL_GOOD;
        }
    }
}