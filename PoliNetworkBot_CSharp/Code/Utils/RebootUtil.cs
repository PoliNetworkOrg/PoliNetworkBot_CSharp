#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
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
            try
            {
                SendMessage.SendMessageInPrivate(sender, sendToSingle, "en",
                    null, text, ParseMode.Html, null, InlineKeyboardMarkup.Empty(),
                    EventArgsContainer.Get(messageEventArgs)).Wait();
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwnersWithLog(e, sender, null, EventArgsContainer.Get(messageEventArgs));
            }
    }

    public static async Task<CommandExecutionState> RebootWithLog(MessageEventArgs? e, TelegramBotAbstract? sender)
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