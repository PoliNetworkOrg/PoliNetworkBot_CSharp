#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class MassiveSendUtil
{
    public static async Task MassiveGeneralSendAsync(MessageEventArgs? e, TelegramBotAbstract sender)
    {
        if (e.Message?.ReplyToMessage == null || (string.IsNullOrEmpty(e.Message.ReplyToMessage.Text) &&
                                                  string.IsNullOrEmpty(e.Message.ReplyToMessage.Caption))
                                              || e.Message.ReplyToMessage.Text == null)
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "You have to reply to a message containing the message" },
                { "it", "You have to reply to a message containing the message" }
            });

            if (e.Message != null)
                await sender.SendTextMessageAsync(e.Message?.From?.Id, text, ChatType.Private,
                    e.Message?.From?.LanguageCode, ParseMode.Html, null, e.Message?.From?.Username,
                    e.Message!.MessageId);
            return;
        }

        var textToBeSent = e.Message.ReplyToMessage.Text;
        var groups = Groups.GetGroupsByTitle("polimi", 1000, sender);
        await MassiveSendSlaveAsync(sender, e, groups, textToBeSent);
    }

    private static async Task<bool> MassiveSendSlaveAsync(TelegramBotAbstract sender, MessageEventArgs? e,
        DataTable? groups, string textToSend)
    {
        await NotifyUtil.NotifyOwners(
            $"WARNING! \n A new massive send has ben authorized by {e?.Message?.From?.Id} [{e?.Message?.From?.Id}] and will be sent in 1000 seconds. \n" +
            $"The message is:\n\n{textToSend}", sender, e);

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

        try
        {
            var g1 = groups.Rows;
            foreach (DataRow element in g1)
            {
                try
                {
                    var groupId = Convert.ToInt64(element.ItemArray[0]);

                    try
                    {
                        await SendMessage.SendMessageInAGroup(sender, "en", new Language(dict2), e, groupId,
                            ChatType.Supergroup, ParseMode.Html, null, default);
                        counter++;
                    }
                    catch
                    {
                        try
                        {
                            await SendMessage.SendMessageInAGroup(sender, "en", new Language(dict2), e,
                                groupId,
                                ChatType.Group, ParseMode.Html, null, default);
                            counter++;
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

        if (e?.Message?.From == null) return true;
        await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
            e.Message.From.LanguageCode,
            ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
        return true;
    }
}