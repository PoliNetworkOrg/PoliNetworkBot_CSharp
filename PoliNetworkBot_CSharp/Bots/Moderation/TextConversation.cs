using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Bots.Moderation
{
    internal class TextConversation
    {
        internal static void DetectMessage(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
            {
                PrivateMessage(telegramBotClient, e);
            }
            else
            {
                //ignore, the message was not sent in private
            }
        }

        private static void PrivateMessage(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            //todo: check user state
            Utils.SendMessage.SendMessageInPrivate(telegramBotClient, e,
                "Ciao, al momento non è possibile fare conversazione col bot.\nTi consigliamo di premere /help per vedere le funzioni disponibili");
        }
    }
}