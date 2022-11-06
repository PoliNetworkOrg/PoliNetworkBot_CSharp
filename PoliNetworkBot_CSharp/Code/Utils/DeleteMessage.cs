#region

using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class DeleteMessage
{
    public static async Task DeleteIfMessageIsNotInPrivate(TelegramBotAbstract? telegramBotClient,
        Message? e)
    {
        if (e != null && e.Chat.Type == ChatType.Private)
            return;

        try
        {
            if (telegramBotClient != null)
                if (e != null)
                    await telegramBotClient.DeleteMessageAsync(e.Chat.Id, e.MessageId, null);
        }
        catch
        {
            // ignored
        }
    }

    internal static void TryDeleteMessagesAsync(MessageList? messages, TelegramBotAbstract? telegramBotClient)
    {
        if (telegramBotClient != null) messages?.TryDeleteMessagesAsync(telegramBotClient);
    }
}