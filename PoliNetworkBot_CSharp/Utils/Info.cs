using System;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class Info
    {
        internal static int? GetTargetUserId(string target, TelegramBotAbstract telegramBotAbstract)
        {
            if (string.IsNullOrEmpty(target))
                return null;

            if (target.StartsWith("-"))
            {
                try
                {
                    return Convert.ToInt32(target);
                }
                catch
                {
                    return null;
                }
            }

            if (target[0] >= '0' && target[0] <= '9')
            {
                try
                {
                    return Convert.ToInt32(target);
                }
                catch
                {
                    return null;
                }
            }

            return GetIDFromUsername(target, telegramBotAbstract);
        }

        private static int? GetIDFromUsername(string target, TelegramBotAbstract telegramBotAbstract)
        {
            return telegramBotAbstract.GetIDFromUsername(target);
        }
    }
}