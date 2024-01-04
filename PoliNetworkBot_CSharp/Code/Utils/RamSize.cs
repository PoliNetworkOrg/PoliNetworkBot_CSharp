#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class RamSize
{
    private static RamUsedCollection? _ramUsedStatic;

    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public static async Task CheckRamSizeThread()
    {
        while (true)
        {
            await CheckRamSizeThread2();
            Thread.Sleep(1000 * 60 * 60 * 1); // every 1 hr
        }
    }

    private static async Task CheckRamSizeThread2()
    {
        try
        {
            var proc = Process.GetCurrentProcess();
            var procPrivateMemorySize64 = proc.PrivateMemorySize64;
            var ramUsed2 = GC.GetTotalMemory(true);
            var ramUsed = new RamUsed(procPrivateMemorySize64, ramUsed2);
            var firstTime = _ramUsedStatic == null;
            _ramUsedStatic ??= new RamUsedCollection();
            _ramUsedStatic.Append(ramUsed);

            if (!_ramUsedStatic.InferioreDi(ramUsed) && firstTime == false) return;

            var backupGroup = GroupsConstants.BackupGroup.FullLong();

            //send info
            await SendRamSize(ramUsed, backupGroup);
            await SendMessageStoreCount(backupGroup);
            await SendMessageThreadsCount(backupGroup);
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex, LogSeverityLevel.ERROR);
        }
    }

    private static async Task SendMessageStoreCount(long backupGroup)
    {
        var storeSizeMessage = "#messageStorageCount " + MessagesStore.GetStoreSize();
        Logger.Logger.WriteLine(storeSizeMessage);
        var language = new Language(
            new Dictionary<string, string?>
            {
                { "en", storeSizeMessage }
            });
        await SendMessage.SendMessageInAGroup(BotUtil.GetFirstModerationRealBot(), "en",
            language, null, backupGroup, ChatType.Group, ParseMode.Html, null, true);
    }

    private static async Task SendMessageThreadsCount(long backupGroup)
    {
        const string messageThreadCount = "messageThreadCount";
        var num = "#" + messageThreadCount + " " + Stats.GetCountStored();
        Logger.Logger.WriteLine(messageThreadCount + ": " + num);
        var language = new L(num);
        await SendMessage.SendMessageInAGroup(BotUtil.GetFirstModerationRealBot(), null,
            language, null, backupGroup, ChatType.Group, ParseMode.Html, null, true);
    }

    private static async Task SendRamSize(RamUsed ramUsed, long backupGroup)
    {
        var message = "#ramsize " + ramUsed;
        Logger.Logger.WriteLine(message);
        var language = new Language(
            new Dictionary<string, string?>
            {
                { "en", message }
            });
        await SendMessage.SendMessageInAGroup(BotUtil.GetFirstModerationRealBot(), "en",
            language, null, backupGroup, ChatType.Group, ParseMode.Html, null, true);
    }
}