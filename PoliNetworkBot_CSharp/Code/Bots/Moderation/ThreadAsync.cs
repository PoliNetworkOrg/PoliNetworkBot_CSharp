using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Threading;

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

            var t4 = new Thread(DoBackup);
            t4.Start();

            var t5 = new Thread(DoCheckAllowedMessageExpiration2Async);
            t5.Start();

            var t6 = new Thread(StartLogger);
            t6.Start();

            //var t3 = new Thread(FixThings);
            //t3.Start();
        }

        private static async void StartLogger()
        {
            await Logger.MainMethodAsync();
        }

        private static void DoBackup()
        {
            var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, Data.Constants.BotStartMethods.Moderation);
            if (bots == null || bots.Count == 0)
                return;

            Thread t = new(() => DoBackup2Async(bots[0]));
            t.Start();
        }

        private static async void DoBackup2Async(TelegramBotAbstract bot)
        {
            if (bot == null)
                return;
            try
            {
                while (true)
                {
                    await CommandDispatcher.BackupHandler(Data.Constants.Groups.BackupGroup, bot, null);
                    Thread.Sleep(1000 * 3600 * 24 * 7);
                }
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, bot);
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
                var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, Data.Constants.BotStartMethods.Moderation);
                if (bots == null || bots.Count == 0)
                    return;
                await NotifyUtil.NotifyOwners(e, bots[0]);
            }
        }

        private static void CheckAllowedMessageExpiration()
        {
            while (true)
            {
                AllowedMessages.CheckTimestamp();
                Thread.Sleep(1000 * 3600 * 24);
            }
        }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private static TelegramBotAbstract GetFirstBot()
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        {
            foreach (var bot in GlobalVariables.Bots.Keys)
            {
                var bot2 = GlobalVariables.Bots[bot];
                switch (bot2.GetBotType())
                {
                    case BotTypeApi.REAL_BOT:
                        return bot2;

                    case BotTypeApi.USER_BOT:
                        break;

                    case BotTypeApi.DISGUISED_BOT:
                        break;
                }
            }

            return null;
        }

        private static async void CheckMessagesToDeleteAsync()
        {
            while (true)
            {
                await MessageDb.CheckMessageToDelete();
                Thread.Sleep(20 * 1000); //20 sec
            }
        }

        private static async void CheckMessagesToSend()
        {
            while (true)
            {
                await MessageDb.CheckMessagesToSend(false, null);
                Thread.Sleep(20 * 1000); //20 sec
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}