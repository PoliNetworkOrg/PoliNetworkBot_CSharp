#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class MassiveSendUtil
{
    public static async Task<bool> MassiveGeneralSendAsync(MessageEventArgs? e, TelegramBotAbstract sender)
    {
        if (e?.Message?.ReplyToMessage == null || (string.IsNullOrEmpty(e.Message.ReplyToMessage.Text) &&
                                                   string.IsNullOrEmpty(e.Message.ReplyToMessage.Caption))
                                               || e.Message.ReplyToMessage.Text == null)
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "You have to reply to a message containing the message" },
                { "it", "You have to reply to a message containing the message" }
            });

            if (e?.Message != null)
                await sender.SendTextMessageAsync(e.Message?.From?.Id, text, ChatType.Private,
                    e.Message?.From?.LanguageCode, ParseMode.Html, null, e.Message?.From?.Username,
                    e.Message!.MessageId);
            return false;
        }

        var textToBeSent = e.Message.ReplyToMessage.Text;
        var groups = Groups.GetGroupsByTitle("polimi", 1000, sender);
        return await MassiveSendSlaveAsync(sender, e, groups, textToBeSent);
    }

    private static async Task<bool> MassiveSendSlaveAsync(TelegramBotAbstract sender, MessageEventArgs? e,
        DataTable? groups, string textToSend)
    {
        await NotifyUtil.NotifyOwners_AnError_AndLog3(
            $"WARNING! \n A new massive send has ben authorized by {e?.Message?.From?.Id} [{e?.Message?.From?.Id}] and will be sent in 1000 seconds. \n" +
            $"The message is:\n\n{textToSend}", sender, e, FileTypeJsonEnum.SIMPLE_STRING);

        Thread.Sleep(1000 * 1000);

        if (groups?.Rows == null || groups.Rows.Count == 0)
        {
            var dict = new Dictionary<string, string?> { { "en", "No groups!" } };
            if (e?.Message?.From != null)
                await sender.SendTextMessageAsync(e.Message.From.Id, new Language(dict), ChatType.Private,
                    e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                    e.Message.MessageId);
            return false;
        }

        var counter = 0;

        var dict2 = new Dictionary<string, string?>
        {
            {
                "en",
                textToSend
            }
        };

        var text2 = new Language(dict2);
        try
        {
            var g1 = groups.Rows;
            foreach (DataRow element in g1)
            {
                var sent = await SendInAGroup(element, sender, text2, e);
                if (sent != null)
                    counter++;

                await Task.Delay(500);
            }
        }
        catch
        {
            // ignored
        }

        var text = new Language(new Dictionary<string, string?>
        {
            { "en", "Sent in  " + counter + " groups" }
        });

        await Task.Delay(500);

        if (e?.Message?.From == null) 
            return true;
        
        await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
            e.Message.From.LanguageCode,
            ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
        return true;
    }

    private static async Task<MessageSentResult?> SendInAGroup(
        DataRow element, TelegramBotAbstract sender, Language text,
        MessageEventArgs? e)
    {
        return null; //todo: remove
        
        try
        {
            var groupId = Convert.ToInt64(element.ItemArray[0]);

            try
            {
                return await SendMessage.SendMessageInAGroup(sender, "en", text, e, groupId,
                    ChatType.Supergroup, ParseMode.Html, null, default);
                
            }
            catch
            {
                try
                {
                    return await SendMessage.SendMessageInAGroup(sender, "en", text, e,
                        groupId,
                        ChatType.Group, ParseMode.Html, null, default);
                   
                }
                catch
                {
                    // ignored
                }
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }
}