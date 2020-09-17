#region

using System.Collections.Generic;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class Blacklist
    {
        internal static SpamType IsSpam(string text)
        {
            if (string.IsNullOrEmpty(text))
                return SpamType.ALL_GOOD;

            var isSpamLink = CheckSpamLink(text);
            return isSpamLink ? SpamType.SPAM_LINK : CheckNotAllowedWords(text);
        }

        private static SpamType CheckNotAllowedWords(string text)
        {
            text = text.ToLower();

            var s = text.Split(' ');
            foreach (var s2 in s)
                switch (s2)
                {
                    case "porcodio":
                    case "dioporco":
                    case "diocane":
                        return SpamType.NOT_ALLOWED_WORDS;
                }

            return SpamType.ALL_GOOD;
            ;
        }

        private static bool CheckSpamLink(string text)
        {
            if (!text.Contains("t.me/"))
                return text.Contains("facebook.com") ||
                       text.Contains("whatsapp.com") ||
                       text.Contains("instagram.com") ||
                       text.Contains("bit.ly") ||
                       text.Contains("is.gd") ||
                       text.Contains("amzn.to") ||
                       text.Contains("goo.gl") ||
                       text.Contains("forms.gle") ||
                       text.Contains("docs.google.com") ||
                       text.Contains("discord.gg");

            if (text.Contains("t.me/c/")) return false;

            var isOurLink = CheckIfIsOurTgLink(text);
            return isOurLink != null && !isOurLink.Value;
        }

        private static bool? CheckIfIsOurTgLink(string text)
        {
            const string q1 = "SELECT id FROM Groups WHERE link = @link";
            var link = GetTelegramLink(text);

            if (string.IsNullOrEmpty(link))
                return null;

            var dt = SqLite.ExecuteSelect(q1, new Dictionary<string, object> {{"@link", link}});
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
            //todo: analizzare la foto con un ocr

            return SpamType.ALL_GOOD;
        }
    }
}