using PoliNetworkBot_CSharp.Code.Objects;
using System;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Info
    {
        internal static async System.Threading.Tasks.Task<int?> GetTargetUserIdAsync(string target, TelegramBotAbstract telegramBotAbstract)
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

            return await GetIDFromUsernameAsync(target, telegramBotAbstract);
        }

        private static async System.Threading.Tasks.Task<int?> GetIDFromUsernameAsync(string target, TelegramBotAbstract telegramBotAbstract)
        {
            return await telegramBotAbstract.GetIDFromUsernameAsync(target);
        }
    }
}