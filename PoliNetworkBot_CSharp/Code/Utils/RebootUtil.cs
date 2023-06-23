#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.Backup;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class RebootUtil
{
    private static async Task AnnounceReboot(TelegramBotAbstract? sender, MessageEventArgs messageEventArgs)
    {
        var sendTo = Logger.Logger.GetLogTo(messageEventArgs);
        var text = new Language(new Dictionary<string, string?>
        {
            { "en", "Reboot by " + UserbotPeer.GetHtmlStringWithUserLink(messageEventArgs.Message.From) }
        });

        foreach (var sendToSingle in sendTo)
        {
            var eventArgsContainer = EventArgsContainer.Get(messageEventArgs);
            try
            {
                var messageMessageThreadId = messageEventArgs.Message.MessageThreadId;
                const ParseMode parseMode = ParseMode.Html;
                var sendMessageInPrivate = SendMessage.SendMessageInPrivate(sender, sendToSingle, "en",
                    null,
                    text, parseMode,
                    null,
                    messageThreadId: messageMessageThreadId,
                    inlineKeyboardMarkup: InlineKeyboardMarkup.Empty(),
                    eventArgsContainer: eventArgsContainer);
                sendMessageInPrivate.Wait();
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwnersWithLog(e, sender, null, eventArgsContainer);
            }
        }
    }

    public static void RebootWithLog(ActionFuncGenericParams actionFuncGenericParams)
    {
        if (e == null)
            return CommandExecutionState.UNMET_CONDITIONS;

        await AnnounceReboot(sender, e);

        try
        {
            Logger.Logger.GetLog(sender, e);
        }
        catch
        {
            // ignored
        }

        return Reboot() ? CommandExecutionState.SUCCESSFUL : CommandExecutionState.ERROR_DEFAULT;
    }

    private static bool Reboot()
    {
        try
        {
            BackupUtil.BackupBeforeReboot();
        }
        catch
        {
            // ignored
        }

        return true;
    }
}