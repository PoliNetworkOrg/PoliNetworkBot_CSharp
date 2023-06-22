#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Conversation;

internal static class TextConversation
{
    internal static async Task<ActionDoneObject> DetectMessage(TelegramBotAbstract? telegramBotClient,
        MessageEventArgs? e)
    {
        if (e?.Message == null)
            return new ActionDoneObject(ActionDoneEnum.NONE, false, null);

        switch (e.Message.Chat.Type)
        {
            case ChatType.Private:
            {
                await PrivateMessage(telegramBotClient, e);
                return new ActionDoneObject(ActionDoneEnum.PRIVATE_MESSAGE_ANSWERED, null, null);
            }
            case ChatType.Channel:
                break;

            case ChatType.Group:
            case ChatType.Supergroup:
            {
                var x = MessageInGroup(telegramBotClient, e);
                return new ActionDoneObject(x, null, null);
            }
            case ChatType.Sender:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new ActionDoneObject(ActionDoneEnum.NONE, false, null);
    }

    private static ActionDoneEnum MessageInGroup(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        if (e?.Message == null)
            return ActionDoneEnum.NONE;

        if (string.IsNullOrEmpty(e.Message.Text))
            return ActionDoneEnum.NONE;

        var text = e.Message.Text.ToLower();
        var title = e.Message.Chat.Title?.ToLower();
        if (string.IsNullOrEmpty(title) || !title.Contains("polimi"))
            return ActionDoneEnum.GROUP_MESSAGE_HANDLED_NONE;

        AutoReplyInGroups.MessageInGroup2Async(telegramBotClient, e, text);
        return ActionDoneEnum.GROUP_MESSAGE_HANDLED_AUTOREPLY;
    }

    private static async Task PrivateMessage(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        if (telegramBotClient != null)
        {
            var botId = telegramBotClient.GetId();

            if (AskUser.UserAnswers.ContainsUser(e?.Message.From?.Id, botId))
                if (AskUser.UserAnswers.GetState(e?.Message.From?.Id, botId) ==
                    AnswerTelegram.State.WAITING_FOR_ANSWER)
                {
                    AskUser.UserAnswers.RecordAnswer(e?.Message.From?.Id, botId,
                        e?.Message.Text ?? e?.Message.Caption);
                    return;
                }
        }

        if (string.IsNullOrEmpty(e?.Message.Text)) return;

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
        var eventArgsContainer = EventArgsContainer.Get(e);
        var eMessage = e.Message;
        var eMessageFrom = eMessage.From;
        await SendMessage.SendMessageInPrivate(telegramBotClient, eMessageFrom?.Id,
            eMessageFrom?.LanguageCode,
            eMessageFrom?.Username, text2,
            ParseMode.Html, null, null, eventArgsContainer, messageThreadId: eMessage.MessageThreadId);
    }
}