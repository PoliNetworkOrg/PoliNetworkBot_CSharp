#region

using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class DeleteMessage
{
    internal static async Task DeleteIfMessageIsNotInPrivate(TelegramBotAbstract telegramBotClient,
        Message e)
    {
        if (e.Chat.Type == ChatType.Private)
            return;

        try
        {
            await telegramBotClient.DeleteMessageAsync(e.Chat.Id, e.MessageId, null);
        }
        catch
        {
            // ignored
        }
    }

    internal static async Task TryDeleteMessagesAsync(List<Message> messages, TelegramBotAbstract telegramBotClient)
    {
        if (messages == null)
            return;

        foreach (var m in messages) await DeleteIfMessageIsNotInPrivate(telegramBotClient, m);
    }
}