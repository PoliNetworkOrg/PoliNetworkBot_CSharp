﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;
using Groups = PoliNetworkBot_CSharp.Code.Data.Constants.Groups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    public class ThreadAsync
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

            var t8 = new Thread(UpdateGroups);
            t8.Start();

            //var t3 = new Thread(FixThings);
            //t3.Start();
        }

        private static void UpdateGroups()
        {
            try
            {
                var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, BotStartMethods.Moderation);
                if (bots == null || bots.Count == 0)
                    return;

                Thread t = new(() => UpdateGroups2(bots[0], null));
                t.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex);
            }
        }

        private static void UpdateGroups2(TelegramBotAbstract bot, MessageEventArgs messageEventArgs)
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
            catch (Exception e)
            {
                Logger.WriteLine(e);
            }
        }

        private static void SayYouRestarted(object obj)
        {
            try
            {
                var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, BotStartMethods.Moderation);
                if (bots == null || bots.Count == 0)
                    return;

                Thread t = new(() => SayYouRestarted2(bots[0]));
                t.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex);
            }
        }

        private static void SayYouRestarted2(TelegramBotAbstract telegramBotAbstract)
        {
            if (telegramBotAbstract == null)
                return;

            try
            {
                Language text = new(new Dictionary<string, string>
                {
                    { "en", "#restarted \nGitHub Build Date:\n" + CommandDispatcher.GetRunningTime().Result }
                });
                _ = telegramBotAbstract.SendTextMessageAsync(Groups.BackupGroup, text, ChatType.Supergroup, "en",
                    ParseMode.Html, null, null);
            }
            catch (Exception ex)
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
                var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, BotStartMethods.Moderation);
                if (bots == null || bots.Count == 0)
                    return;

                Thread t = new(() => DoBackupAndMessageStore2Async(bots[0], null));
                t.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex);
            }
        }

        private static async void DoBackupAndMessageStore2Async(TelegramBotAbstract bot,
            MessageEventArgs messageEventArgs)
        {
            if (bot == null)
                return;
            try
            {
                while (true)
                {
                    await CommandDispatcher.BackupHandler(Groups.BackupGroup, bot, null, ChatType.Group);
                    Thread.Sleep(1000 * 3600 * 24 * 7);
                    _ = File.WriteAllTextAsync("", Paths.Data.MessageStore);
                }
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, bot, messageEventArgs);
            }
        }

        private static async void DoCheckAllowedMessageExpiration2Async()
        {
            try
            {
                CheckAllowedMessageExpiration();
            }
            catch (Exception e)
            {
                var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, BotStartMethods.Moderation);
                if (bots == null || bots.Count == 0)
                    return;
                await NotifyUtil.NotifyOwners(e, bots[0], null);
            }
        }

        private static void CheckAllowedMessageExpiration()
        {
            while (true)
            {
                MessagesStore.CheckTimestamp();
                Thread.Sleep(1000 * 3600 * 24);
            }
        }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private static TelegramBotAbstract GetFirstBot()
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        {
            foreach (var bot2 in GlobalVariables.Bots.Keys.Select(bot => GlobalVariables.Bots[bot]))
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
                catch (Exception e)
                {
                    _ = NotifyUtil.NotifyOwners(e, GetFirstBot(), null);
                }

                try
                {
                    Thread.Sleep(20 * 1000); //20 sec
                }
                catch
                {
                    ;
                }
            }
        }

        private static async void CheckMessagesToSend()
        {
            while (true)
            {
                await MessageDb.CheckMessagesToSend(false, null, null);
                Thread.Sleep(20 * 1000); //20 sec
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}