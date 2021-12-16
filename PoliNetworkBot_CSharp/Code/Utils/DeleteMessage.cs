﻿#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Objects;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class DeleteMessage
    {
        internal static async Task DeleteIfMessageIsNotInPrivate(TelegramBotAbstract telegramBotClient,
            MessageEventArgs e)
        {
            if (e.Message.Chat.Type == ChatType.Private)
                return;

            try
            {
                await telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, null);
            }
            catch
            {
                // ignored
            }
        }
    }
}