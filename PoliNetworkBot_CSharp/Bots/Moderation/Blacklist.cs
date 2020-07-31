using System.Data;

namespace PoliNetworkBot_CSharp.Bots.Moderation
{
    internal class Blacklist
    {
        internal static SpamType IsSpam(string text)
        {
            if (string.IsNullOrEmpty(text))
                return SpamType.ALL_GOOD;

            bool isSpamLink = CheckSpamLink(text);
            if (isSpamLink)
                return SpamType.SPAM_LINK;

            return CheckNotAllowedWords(text);
        }

        private static SpamType CheckNotAllowedWords(string text)
        {
            text = text.ToLower();

            var s = text.Split(' ');
            foreach (var s2 in s)
            {
                switch (s2)
                {
                    case "porcodio":
                    case "dioporco":
                    case "diocane":
                        return SpamType.NOT_ALLOWED_WORDS;
                }
            }

            return SpamType.ALL_GOOD;
        }

        private static bool CheckSpamLink(string text)
        {
            if (text.Contains("t.me/"))
            {
                if (text.Contains("t.me/c/"))
                {
                    return false;
                }

                bool? is_our_link = CheckIfIsOurTgLink(text);
                if (is_our_link == null || is_our_link.Value == true)
                    return false;
                else
                    return true;
            }

            if (
                    text.Contains("facebook.com") ||
                    text.Contains("whatsapp.com") ||
                    text.Contains("instagram.com") ||
                    text.Contains("bit.ly") ||
                    text.Contains("is.gd") ||
                    text.Contains("amzn.to") ||
                    text.Contains("goo.gl") ||
                    text.Contains("forms.gle") ||
                    text.Contains("docs.google.com") ||
                    text.Contains("discord.gg")
                )
                return true;

            return false;
        }

        private static bool? CheckIfIsOurTgLink(string text)
        {
            string q1 = "SELECT id FROM Groups WHERE link = @link";
            string link = GetTelegramLink(text);

            if (string.IsNullOrEmpty(link))
                return null;

            DataTable dt = Utils.SQLite.ExecuteSelect(q1, new System.Collections.Generic.Dictionary<string, object>() { { "@link", link } });
            var value = Utils.SQLite.GetFirstValueFromDataTable(dt);
            if (value == null)
                return false;
            string s = value.ToString();
            if (string.IsNullOrEmpty(s))
                return false;

            return true;
        }

        private static string GetTelegramLink(string text)
        {
            var s = text.Split(' ');
            foreach (var s2 in s)
            {
                if (s2.Contains("t.me/"))
                {
                    return s2;
                }
            }

            return null;
        }
    }
}