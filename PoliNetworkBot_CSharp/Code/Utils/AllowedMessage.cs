using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class AllowedMessage
{
    public static async Task<CommandExecutionState> GetAllowedMessages(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "List of messages: " },
            { "en", "List of messages: " }
        });
        if (sender == null)
            return CommandExecutionState.UNMET_CONDITIONS;


        if (e != null)
        {
            var message1 = e.Message;


            var messageOptions = new MessageOptions

            {
                ChatType = ChatType.Private,
                ChatId = e.Message.From?.Id,
                Text = text,
                Lang = e.Message.From?.LanguageCode,
                Username = e.Message.From?.Username,
                ReplyToMessageId = message1.MessageId
            };
            await sender.SendTextMessageAsync(messageOptions);
        }

        var messages = MessagesStore.GetAllMessages(x =>
            x != null && x.AllowedStatus.GetStatus() == MessageAllowedStatusEnum.ALLOWED);
        if (messages == null)
            return CommandExecutionState.UNMET_CONDITIONS;

        foreach (var m2 in messages.Select(message => message?.message)
                     .Where(m2 => m2 != null))
        {
            text = new Language(new Dictionary<string, string?>
            {
                { "uni", m2 }
            });

            var messageOptions = new MessageOptions

            {
                ChatType = ChatType.Private,
                ChatId = e?.Message?.From?.Id,
                Text = text,
                Lang = "uni",

                Username = e?.Message?.From?.Username
            };
            await sender.SendTextMessageAsync(messageOptions);
        }


        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> UnAllowMessage(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var message = e?.Message;
        if (message == null ||
            e == null ||
            !Owners.CheckIfOwner(e.Message.From?.Id) ||
            message.Chat.Type != ChatType.Private)
            return CommandExecutionState.UNMET_CONDITIONS;

        if (e.Message.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "You have to reply to a message containing the message" }
            });
            if (sender == null)
                return CommandExecutionState.UNMET_CONDITIONS;
            var o = e.Message;


            var messageOptions = new MessageOptions

            {
                ChatType = ChatType.Private,
                ChatId = e.Message.From?.Id,
                Text = text,
                Lang = e.Message.From?.LanguageCode,
                Username = e.Message.From?.Username,
                ReplyToMessageId = o.MessageId
            };
            await sender.SendTextMessageAsync(messageOptions);

            return CommandExecutionState.UNMET_CONDITIONS;
        }

        MessagesStore.RemoveMessage(e.Message.ReplyToMessage.Text);
        return CommandExecutionState.SUCCESSFUL;
    }
}