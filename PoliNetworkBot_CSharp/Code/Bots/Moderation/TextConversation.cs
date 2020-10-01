#region

using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class TextConversation
    {
        internal static async Task DetectMessage(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == ChatType.Private) await PrivateMessage(telegramBotClient, e);
        }

        private static async Task PrivateMessage(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            if (AskUser.UserAnswers.ContainsKey(e.Message.From.Id))
                if (AskUser.UserAnswers[e.Message.From.Id] != null)
                    if (AskUser.UserAnswers[e.Message.From.Id].GetState() == AnswerTelegram.State.WAITING_FOR_ANSWER)
                    {
                        AskUser.UserAnswers[e.Message.From.Id].RecordAnswer(e.Message.Text);
                        return;
                    }


            var text2 = new Language(new Dictionary<string, string>
            {
                {
                    "en",
                    "Hi, at the moment is not possible to have conversation with the bot.\n" +
                    "We advice you to write /help to see what this bot can do"
                },
                {
                    "it",
                    "Ciao, al momento non è possibile fare conversazione col bot.\n" +
                    "Ti consigliamo di premere /help per vedere le funzioni disponibili"
                }
            });
            await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
e.Message.From.LanguageCode,
e.Message.From.Username, text2);
        }
    }
}