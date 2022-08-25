#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Conversation;

internal static class TextConversation
{
    internal static async Task DetectMessage(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        if (e?.Message != null)
            switch (e.Message.Chat.Type)
            {
                case ChatType.Private:
                {
                    await PrivateMessage(telegramBotClient, e);
                    break;
                }
                case ChatType.Channel:
                    break;

                case ChatType.Group:
                case ChatType.Supergroup:
                {
                    await MessageInGroup(telegramBotClient, e);
                    break;
                }
                case ChatType.Sender:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
    }

    private static async Task MessageInGroup(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        if (e?.Message == null)
            return;

        if (string.IsNullOrEmpty(e.Message.Text))
            return;

        var text = e.Message.Text.ToLower();
        var title = e.Message?.Chat.Title?.ToLower();
        if (string.IsNullOrEmpty(title) == false && title.Contains("polimi"))
            AutoReplyInGroups.MessageInGroup2Async(telegramBotClient, e, text);
    }

    private static async Task PrivateMessage(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        if (telegramBotClient != null)
        {
            var botId = telegramBotClient.GetId();

            if (AskUser.UserAnswers.ContainsUser(e?.Message?.From?.Id, botId))
                if (AskUser.UserAnswers.GetState(e?.Message?.From?.Id, botId) ==
                    AnswerTelegram.State.WAITING_FOR_ANSWER)
                {
                    AskUser.UserAnswers.RecordAnswer(e?.Message?.From?.Id, botId,
                        e?.Message?.Text ?? e?.Message?.Caption);
                    return;
                }
        }

        if (string.IsNullOrEmpty(e?.Message?.Text)) return;

        var text2 = new Language(new Dictionary<string, string?>
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
        await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From?.Id,
            e.Message.From?.LanguageCode,
            e.Message.From?.Username, text2,
            ParseMode.Html, null);
    }
}