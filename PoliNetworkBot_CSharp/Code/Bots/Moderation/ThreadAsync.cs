using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Threading;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    public class ThreadAsync
    {
        public static void DoThingsAsyncBot(object obj)
        {
            var t = new Thread(CheckMessagesToSend);
            t.Start();

            var t2 = new Thread(CheckMessagesToDeleteAsync);
            t2.Start();

            var t3 = new Thread(FixThings);
            t3.Start();

        }

        private static  async void FixThings()
        {
            try
            {
                TelegramBotAbstract bot = GlobalVariables.Bots[768169879];
                if (bot == null)
                    return;

                try
                {
                    await bot.DeleteMessageAsync(-1001314601927, 30, Telegram.Bot.Types.Enums.ChatType.Channel, null);
                }
                catch
                {
                    ;
                }

                try
                {
                    await bot.DeleteMessageAsync(-1001314601927, 31, Telegram.Bot.Types.Enums.ChatType.Channel, null);
                }
                catch
                {
                    ;
                }

                try
                {
                    await bot.DeleteMessageAsync(-1001314601927, 30, Telegram.Bot.Types.Enums.ChatType.Supergroup, null);
                }
                catch
                {
                    ;
                }

                try
                {
                    await bot.DeleteMessageAsync(-1001314601927, 31, Telegram.Bot.Types.Enums.ChatType.Supergroup, null);
                }
                catch
                {
                    ;
                }
            }
            catch (Exception e)
            {
                Utils.NotifyUtil.NotifyOwners(e, GetFirstBot());
            }
        }

        private static TelegramBotAbstract GetFirstBot()
        {
            foreach (var bot in Code.Data.GlobalVariables.Bots.Keys)
            {
                var bot2 = Code.Data.GlobalVariables.Bots[bot];
                switch (bot2.GetBotType())
                {
                    case Enums.BotTypeApi.REAL_BOT:
                        return bot2;
                    case Enums.BotTypeApi.USER_BOT:
                        break;
                    case Enums.BotTypeApi.DISGUISED_BOT:
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
                await MessageDb.CheckMessagesToSend();
                Thread.Sleep(20 * 1000); //20 sec
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}