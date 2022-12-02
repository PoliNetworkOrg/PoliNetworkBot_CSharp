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

            await sender.SendTextMessageAsync(e.Message.From?.Id, text,
                ChatType.Private,
                e.Message.From?.LanguageCode, ParseMode.Html, null,
                e.Message.From?.Username,
                message1.MessageId);
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
            await sender.SendTextMessageAsync(e?.Message?.From?.Id, text,
                ChatType.Private,
                "uni", ParseMode.Html, null, e?.Message?.From?.Username);
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
            await sender.SendTextMessageAsync(e.Message.From?.Id, text,
                ChatType.Private,
                e.Message.From?.LanguageCode, ParseMode.Html, null,
                e.Message.From?.Username,
                o.MessageId);

            return CommandExecutionState.UNMET_CONDITIONS;
        }

        MessagesStore.RemoveMessage(e.Message.ReplyToMessage.Text);
        return CommandExecutionState.SUCCESSFUL;
    }
}