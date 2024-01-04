#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Backup;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation;

public static class ThreadAsyncModeration
{
    public static void DoThingsAsyncBot()
    {
        var t = new Thread(CheckMessagesToSend);
        t.Start();

        var t2 = new Thread(CheckMessagesToDeleteAsync);
        t2.Start();

        var t4 = new Thread(DoBackupAndMessageStore);
        t4.Start();

        var t5 = new Thread(DoCheckAllowedMessageExpiration2Async);
        t5.Start();

        var t6 = new Thread(StartLogger);
        t6.Start();

        var t7 = new Thread(SayYouRestarted);
        t7.Start();

        // var t8 = new Thread(UpdateGroups);
        // t8.Start();

        var t9 = new Thread(DoCheckCallbackDataExpired);
        t9.Start();

        var t10 = new Thread(AutomaticLog);
        t10.Start();

        var t11 = new Thread(CheckRamSize);
        t11.Start();

        var t12 = new Thread(ProgressiveLinkCheck);
        t12.Start();

        //var t3 = new Thread(FixThings);
        //t3.Start();
    }

    private static void ProgressiveLinkCheck()
    {
        try
        {
            Groups.ProgressiveLinkCheck();
        }
        catch (Exception e)
        {
            Logger.WriteLine(e);
        }
    }

    private static async void CheckRamSize()
    {
        try
        {
            await RamSize.CheckRamSizeThread();
        }
        catch
        {
            // ignored
        }
    }

    private static void AutomaticLog()
    {
        try
        {
            Logger.AutomaticLog();
        }
        catch
        {
            // ignored
        }
    }

    private static void DoCheckCallbackDataExpired(object? obj)
    {
        CallbackUtils.DoCheckCallbackDataExpired();
    }

    private static void UpdateGroups()
    {
        try
        {
            var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, BotStartMethods.Moderation.Item1);
            if (bots == null || bots.Count == 0)
                return;

            Thread t = new(() => UpdateGroups2(bots[0], null));
            t.Start();
        }
        catch (Exception? ex)
        {
            Logger.WriteLine(ex);
        }
    }

    private static void UpdateGroups2(TelegramBotAbstract? bot, MessageEventArgs? messageEventArgs)
    {
        if (bot == null)
        {
            Logger.WriteLine("Automatic routine failed", LogSeverityLevel.ERROR);
            return;
        }

        try
        {
            Logger.WriteLine("Started automatic groups update routine");
            while (true)
            {
                if (DateTime.Now.Hour == 3)
                {
                    Logger.WriteLine("Started automatic groups update", LogSeverityLevel.ALERT);
                    _ = CommandDispatcher.UpdateGroups(bot, false, true, false, messageEventArgs);
                    Thread.Sleep(1000 * 3600 * 60 * 6);
                }

                Thread.Sleep(1000 * 3600 * 59);
            }
        }
        catch (Exception? e)
        {
            Logger.WriteLine(e);
        }
    }

    private static void SayYouRestarted(object? obj)
    {
        try
        {
            var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, BotStartMethods.Moderation.Item1);
            if (bots == null || bots.Count == 0)
                return;

            var bot2 = bots[0];
            Thread t = new(() =>
            {
                if (bot2 != null)
                    SayYouRestarted2(bot2);
            });
            t.Start();
        }
        catch (Exception? ex)
        {
            Logger.WriteLine(ex);
        }
    }

    private static void SayYouRestarted2(TelegramBotAbstract? telegramBotAbstract)
    {
        if (telegramBotAbstract == null)
            return;

        try
        {
            Language text = new(new Dictionary<string, string?>
            {
                { "en", "#restarted \nGitHub Build Date:\n" + CommandDispatcher.GetRunningTime().Result }
            });

            var messageOptions = new MessageOptions

            {
                ChatId = GroupsConstants.BackupGroup.FullLong(),

                Text = text,
                ChatType = ChatType.Supergroup
            };
            _ = telegramBotAbstract.SendTextMessageAsync(messageOptions);
        }
        catch (Exception? ex)
        {
            Logger.WriteLine(ex);
        }
    }

    private static async void StartLogger()
    {
        await Logger.MainMethodAsync();
    }

    private static void DoBackupAndMessageStore()
    {
        try
        {
            var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, BotStartMethods.Moderation.Item1);
            if (bots == null || bots.Count == 0)
                return;

            Thread t = new(() => DoBackupAndMessageStore2Async(bots[0], null));
            t.Start();
        }
        catch (Exception? ex)
        {
            Logger.WriteLine(ex);
        }
    }

    private static async void DoBackupAndMessageStore2Async(TelegramBotAbstract? bot,
        MessageEventArgs? messageEventArgs)
    {
        if (bot == null)
            return;
        try
        {
            while (true)
            {
                await BackupUtil.BackupHandler(new List<long?> { GroupsConstants.BackupGroup.FullLong() }, bot, null,
                    ChatType.Group);
                Thread.Sleep(1000 * 3600 * 24 * 7);
                _ = File.WriteAllTextAsync("", Paths.Data.MessageStore);
            }
        }
        catch (Exception? e)
        {
            try
            {
                await NotifyUtil.NotifyOwnerWithLog2(e, bot, EventArgsContainer.Get(messageEventArgs));
            }
            catch (Exception e2)
            {
                Logger.WriteLine(e, LogSeverityLevel.CRITICAL);
                Logger.WriteLine(e2, LogSeverityLevel.CRITICAL);
            }
        }
    }

    private static async void DoCheckAllowedMessageExpiration2Async()
    {
        try
        {
            CheckAllowedMessageExpiration();
        }
        catch (Exception? e)
        {
            var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, BotStartMethods.Moderation.Item1);
            if (bots == null || bots.Count == 0)
                return;
            await NotifyUtil.NotifyOwnerWithLog2(e, bots[0], null);
        }
    }

    private static void CheckAllowedMessageExpiration()
    {
        while (true)
        {
            MessagesStore.CheckTimestamp();
            Thread.Sleep(1000 * 3600 * 24);
        }
        // ReSharper disable once FunctionNeverReturns
    }


    private static TelegramBotAbstract? GetFirstBot()
    {
        if (GlobalVariables.Bots == null) return null;
        foreach (var bot2 in GlobalVariables.Bots.Keys.Select(bot => GlobalVariables.Bots[bot]))
            if (bot2 != null)
                switch (bot2.GetBotType())
                {
                    case BotTypeApi.REAL_BOT:
                        return bot2;

                    case BotTypeApi.USER_BOT:
                        break;

                    case BotTypeApi.DISGUISED_BOT:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

        return null;
    }

    private static async void CheckMessagesToDeleteAsync()
    {
        while (true)
        {
            try
            {
                await MessageDb.CheckMessageToDelete(null);
            }
            catch (Exception? e)
            {
                _ = NotifyUtil.NotifyOwnerWithLog2(e, GetFirstBot(), null);
            }

            try
            {
                Thread.Sleep(20 * 1000); //20 sec
            }
            catch
            {
                // ignored
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static async void CheckMessagesToSend()
    {
        while (true)
        {
            try
            {
                await MessageDb.CheckMessagesToSend(false, BotUtil.GetFirstModerationRealBot(), null);
            }
            catch (Exception? ex)
            {
                Logger.WriteLine(ex);
            }

            Thread.Sleep(20 * 1000); //20 sec
        }

        // ReSharper disable once FunctionNeverReturns
    }
}