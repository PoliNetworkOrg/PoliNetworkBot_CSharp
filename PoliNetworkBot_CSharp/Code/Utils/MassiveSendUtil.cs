#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class MassiveSendUtil
{
    private static async Task<bool> MassiveGeneralSendAsync(MessageEventArgs? e, TelegramBotAbstract sender, bool test)
    {
        try
        {
            if (e?.Message.ReplyToMessage == null || (string.IsNullOrEmpty(e.Message.ReplyToMessage.Text) &&
                                                      string.IsNullOrEmpty(e.Message.ReplyToMessage.Caption))
                                                  || e.Message.ReplyToMessage.Text == null)
            {
                var text = new Language(new Dictionary<string, string?>
                {
                    { "en", "You have to reply to a message containing the message" },
                    { "it", "You have to reply to a message containing the message" }
                });

                if (e?.Message == null) return false;

                var messageOptions = new MessageOptions
                {
                    ChatId = e.Message.From?.Id,
                    Text = text,
                    ChatType = ChatType.Private,
                    Lang = e.Message.From?.LanguageCode,
                    Username = e.Message.From?.Username,
                    ReplyToMessageId = e.Message.MessageId
                };
                await sender.SendTextMessageAsync(messageOptions);

                return false;
            }

            var textToBeSent = e.Message.ReplyToMessage.Text;
            Logger.Logger.WriteLine("textToBeSent " + textToBeSent);
            var groups = Groups.GetGroupsByTitle("polimi", 1000, sender);
            var rowsCount = groups?.Rows.Count ?? -1;
            Logger.Logger.WriteLine("rowsCount " + rowsCount);
            return await MassiveSendSlaveAsync(sender, e, groups, textToBeSent, test);
        }
        catch (Exception ex)
        {
            await NotifyUtil.NotifyOwnersWithLog(ex, sender, null, EventArgsContainer.Get(e));
        }

        return false;
    }

    public static async Task<bool> MassiveSendSlaveAsync(TelegramBotAbstract sender, MessageEventArgs? e,
        DataTable? groups, string textToSend, bool test)
    {
        await NotifyUtil.NotifyOwners_AnError_AndLog3(
            "WARNING! \n A new massive send has ben authorized by " +
            UserbotPeer.GetHtmlStringWithUserLink(e?.Message.From) + " and will be sent in 1000 seconds. \n" +
            $"The message is:\n\n{textToSend}", sender, EventArgsContainer.Get(e), FileTypeJsonEnum.SIMPLE_STRING,
            SendActionEnum.SEND_TEXT);

        Thread.Sleep(1000 * 1000);

        if (groups?.Rows == null || groups.Rows.Count == 0)
        {
            var dict = new Dictionary<string, string?> { { "en", "No groups!" } };
            if (e?.Message.From == null) return false;

            var messageOptions = new MessageOptions
            {
                ChatId = e.Message.From?.Id,
                Text = new Language(dict),
                ChatType = ChatType.Private,
                Lang = e.Message.From?.LanguageCode,
                Username = e.Message.From?.Username,
                ReplyToMessageId = e.Message.MessageId
            };
            await sender.SendTextMessageAsync(messageOptions);

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
                var sent = await SendInAGroup(element, sender, text2, e, test);
                if (sent != null || test)
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

        if (e?.Message.From == null)
            return true;


        var messageOptions2 = new MessageOptions

        {
            ChatId = e.Message.From?.Id,
            Text = text,
            ChatType = ChatType.Private,
            Lang = e.Message.From?.LanguageCode,
            Username = e.Message.From?.Username,
            ReplyToMessageId = e.Message.MessageId
        };
        await sender.SendTextMessageAsync(messageOptions2);
        return true;
    }

    private static async Task<MessageSentResult?> SendInAGroup(DataRow element, TelegramBotAbstract sender,
        Language text,
        MessageEventArgs? e, bool test)
    {
        if (test)
            return null;

        try
        {
            var groupId = Convert.ToInt64(element.ItemArray[0]);

            try
            {
                return await SendMessage.SendMessageInAGroup(sender, "en", text, EventArgsContainer.Get(e), groupId,
                    ChatType.Supergroup, ParseMode.Html, null, default);
            }
            catch
            {
                try
                {
                    return await SendMessage.SendMessageInAGroup(sender, "en", text, EventArgsContainer.Get(e),
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

    public static async Task<CommandExecutionState> MassiveGeneralSendAsyncCommand(MessageEventArgs? e,
        TelegramBotAbstract? sender)
    {
        return sender == null ? CommandExecutionState.UNMET_CONDITIONS :
            await MassiveGeneralSendAsync(e, sender, false) ? CommandExecutionState.ERROR_DEFAULT :
            CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> MassiveGeneralSendAsyncTestCommand(MessageEventArgs? e,
        TelegramBotAbstract? sender)
    {
        return sender == null ? CommandExecutionState.UNMET_CONDITIONS :
            await MassiveGeneralSendAsync(e, sender, true) ? CommandExecutionState.ERROR_DEFAULT :
            CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> MassiveSend(MessageEventArgs? e, TelegramBotAbstract? sender,
        string[]? cmdLines)
    {
        try
        {
            if (e != null && Matches(GlobalVariables.AllowedBanAll, e.Message.From))
            {
                if (sender == null || e.Message.ReplyToMessage?.Text == null || e.Message.From == null)
                    return CommandExecutionState.UNMET_CONDITIONS;
                await CommandDispatcher.MassiveSendAsync(sender, e, e.Message.ReplyToMessage.Text);
                return CommandExecutionState.SUCCESSFUL;
            }

            await CommandDispatcher.DefaultCommand(sender, e);
        }
        catch (Exception ex)
        {
            Logger.Logger.WriteLine(ex);
        }

        return CommandExecutionState.ERROR_DEFAULT;
    }

    private static bool Matches(IReadOnlyCollection<TelegramUser>? allowedBanAll, User? user)
    {
        return allowedBanAll != null && allowedBanAll.Any(item => item.Matches(user));
    }
}