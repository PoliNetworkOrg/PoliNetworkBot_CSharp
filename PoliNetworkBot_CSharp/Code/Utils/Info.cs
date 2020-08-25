#region

using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Info
    {
        internal static async Task<int?> GetTargetUserIdAsync(string target, TelegramBotAbstract telegramBotAbstract)
        {
            if (string.IsNullOrEmpty(target))
                return null;

            if (target.StartsWith("-"))
                try
                {
                    return Convert.ToInt32(target);
                }
                catch
                {
                    return null;
                }

            if (target[0] >= '0' && target[0] <= '9')
                try
                {
                    return Convert.ToInt32(target);
                }
                catch
                {
                    return null;
                }

            return await GetIDFromUsernameAsync(target, telegramBotAbstract);
        }

        private static async Task<int?> GetIDFromUsernameAsync(string target, TelegramBotAbstract telegramBotAbstract)
        {
            return await telegramBotAbstract.GetIDFromUsernameAsync(target);
        }
    }
}