using System;
using TLSharp.Core;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class UserbotConnect
    {
        internal static async System.Threading.Tasks.Task<TelegramClient> ConnectAsync(UserBotInfo userbot)
        {
            TelegramClient telegramClient =  new TLSharp.Core.TelegramClient(userbot.GetApiId(), userbot.GetApiHash(), sessionUserId: userbot.GetSessionUserId());
            await telegramClient.ConnectAsync();
            return telegramClient;
        }
    }
}