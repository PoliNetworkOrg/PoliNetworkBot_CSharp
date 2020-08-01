using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class DeleteMessage
    {
        internal static void DeleteIfMessageIsNotInPrivate(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            if (e.Message.Chat.Type != Telegram.Bot.Types.Enums.ChatType.Private)
            {
                try
                {
                    telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);
                }
                catch
                {
                    ;
                }
            }
        }
    }
}