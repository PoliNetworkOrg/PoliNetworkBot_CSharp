using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot.Types.Enums;

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

        private static async void FixThings()
        {
            try
            {
                var bot = GlobalVariables.Bots[768169879];
                if (bot == null)
                    return;

                try
                {
                    await bot.DeleteMessageAsync(-1001314601927, 32, ChatType.Channel, null);
                }
                catch
                {
                    ;
                }

                try
                {
                    await bot.DeleteMessageAsync(-1001314601927, 32, ChatType.Channel, null);
                }
                catch
                {
                    ;
                }
                /*
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
                */

                var toSend = "<a href='tg://resolve?domain=-1393901944'>PoliAssociazioni 3</a>";
                var dict = new Dictionary<string, string>
                {
                    {"it", toSend}
                };
                var text = new Language(dict);
                await bot.SendTextMessageAsync(GlobalVariables.Owners[0].Item1, text, ChatType.Private, "it",
                    ParseMode.Html,
                    null, "armef97", null, true);

                /*
                await bot.PromoteChatMember(userId: 149620444, //raif
                     chatId: -1001314601927);
                */
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, GetFirstBot());
            }
        }

        private static TelegramBotAbstract GetFirstBot()
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