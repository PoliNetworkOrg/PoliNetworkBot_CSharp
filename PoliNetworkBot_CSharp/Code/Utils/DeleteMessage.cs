﻿#region

using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class DeleteMessage
    {
        internal static void DeleteIfMessageIsNotInPrivate(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == ChatType.Private) 
                return;
            
            try
            {
                telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, e.Message.Chat.Type);
            }
            catch
            {
                // ignored
            }
        }
    }
}