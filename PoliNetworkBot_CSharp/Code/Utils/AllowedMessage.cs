using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class AllowedMessage
{
    public static async Task<bool> GetAllowedMessages(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "List of messages: " },
            { "en", "List of messages: " }
        });
        if (sender == null)
            return false;


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
            return false;

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


        return false;
    }

    public static async Task<bool> UnAllowMessage(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var message = e?.Message;
        if (message != null &&
            e != null &&
            Owners.CheckIfOwner(e.Message.From?.Id) &&
            message.Chat.Type == ChatType.Private)
        {
            if (e.Message.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
            {
                var text = new Language(new Dictionary<string, string?>
                {
                    { "en", "You have to reply to a message containing the message" }
                });
                if (sender == null)
                    return false;
                var o = e.Message;
                await sender.SendTextMessageAsync(e.Message.From?.Id, text,
                    ChatType.Private,
                    e.Message.From?.LanguageCode, ParseMode.Html, null,
                    e.Message.From?.Username,
                    o.MessageId);

                return false;
            }

            MessagesStore.RemoveMessage(e.Message.ReplyToMessage.Text);
            return false;
        }

        await CommandDispatcher.DefaultCommand(sender, e);

        return false;
    }
}