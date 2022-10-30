#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
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
                    null, text, ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(messageEventArgs)).Wait();
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwnersWithLog(e, sender, null, EventArgsContainer.Get(messageEventArgs));
            }
    }

    public static async Task<bool> RebootWithLog(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (e == null)
            return false;

        await AnnounceReboot(sender, e);

        try
        {
            Logger.Logger.GetLog(sender, e);
        }
        catch
        {
            // ignored
        }

        return Reboot();
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

        using var powershell = PowerShell.Create();
        if (ScriptUtil.DoScript(powershell, "screen -ls", true).Aggregate("", (current, a) => current + a)
            .Contains("rebooter"))
            return false;

        ScriptUtil.DoScript(powershell, "screen -d -m -S rebooter ./static/rebooter.sh", true);

        return true;
    }
}