using System.Collections.Generic;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using SampleNuGet.Objects;
using SampleNuGet.Utils;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class AllowedMessage
{
    public static void GetAllowedMessages(ActionFuncGenericParams actionFuncGenericParams)
    {
        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "List of messages: " },
            { "en", "List of messages: " }
        });
        if (actionFuncGenericParams.TelegramBotAbstract == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }


        if (actionFuncGenericParams.MessageEventArgs != null)
        {
            var message1 = actionFuncGenericParams.MessageEventArgs.Message;

            var sendTextMessageAsync = actionFuncGenericParams.TelegramBotAbstract.SendTextMessageAsync(
                actionFuncGenericParams.MessageEventArgs.Message.From?.Id, text,
                ChatType.Private,
                actionFuncGenericParams.MessageEventArgs.Message.From?.LanguageCode, ParseMode.Html, null,
                actionFuncGenericParams.MessageEventArgs.Message.From?.Username,
                message1.MessageId);
            sendTextMessageAsync.Wait();
        }

        var messages = MessagesStore.GetAllMessages(x =>
            x != null && x.AllowedStatus.GetStatus() == MessageAllowedStatusEnum.ALLOWED);
        if (messages == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        foreach (var m2 in messages.Select(message => message?.message)
                     .Where(m2 => m2 != null))
        {
            text = new Language(new Dictionary<string, string?>
            {
                { "uni", m2 }
            });
            var eMessage = actionFuncGenericParams.MessageEventArgs?.Message;
            var sendTextMessageAsync = actionFuncGenericParams.TelegramBotAbstract.SendTextMessageAsync(
                eMessage?.From?.Id, text,
                ChatType.Private,
                "uni", ParseMode.Html, null, eMessage?.From?.Username, eMessage?.MessageThreadId);
            sendTextMessageAsync.Wait();
        }

        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void UnAllowMessage(ActionFuncGenericParams actionFuncGenericParams)
    {
        var message = actionFuncGenericParams.MessageEventArgs?.Message;
        if (message == null ||
            actionFuncGenericParams.MessageEventArgs == null ||
            !Owners.CheckIfOwner(actionFuncGenericParams.MessageEventArgs.Message.From?.Id) ||
            message.Chat.Type != ChatType.Private)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        if (actionFuncGenericParams.MessageEventArgs.Message.ReplyToMessage == null ||
            string.IsNullOrEmpty(actionFuncGenericParams.MessageEventArgs.Message.ReplyToMessage.Text))
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "You have to reply to a message containing the message" }
            });
            if (actionFuncGenericParams.TelegramBotAbstract == null)
            {
                actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
                return;
            }

            var o = actionFuncGenericParams.MessageEventArgs.Message;
            var sendTextMessageAsync = actionFuncGenericParams.TelegramBotAbstract.SendTextMessageAsync(
                actionFuncGenericParams.MessageEventArgs.Message.From?.Id, text,
                ChatType.Private,
                actionFuncGenericParams.MessageEventArgs.Message.From?.LanguageCode, ParseMode.Html, null,
                actionFuncGenericParams.MessageEventArgs.Message.From?.Username,
                o.MessageId);
            sendTextMessageAsync.Wait();

            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        MessagesStore.RemoveMessage(actionFuncGenericParams.MessageEventArgs.Message.ReplyToMessage.Text);
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }
}