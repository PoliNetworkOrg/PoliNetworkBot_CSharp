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
    }
}