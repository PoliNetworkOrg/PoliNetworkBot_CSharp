using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
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
            if (AskUser.userAnswers.ContainsKey(e.Message.From.Id))
            {
                if (AskUser.userAnswers[e.Message.From.Id] != null)
                {
                    if (AskUser.userAnswers[e.Message.From.Id].GetState() == Objects.AnswerTelegram.State.WaitingForAnswer)
                    {
                        AskUser.userAnswers[e.Message.From.Id].RecordAnswer(e.Message.Text);
                        return;
                    }
                }
            }

            //todo: check user state
            Utils.SendMessage.SendMessageInPrivate(telegramBotClient, e,
                "Ciao, al momento non è possibile fare conversazione col bot.\nTi consigliamo di premere /help per vedere le funzioni disponibili");
        }
    }
}