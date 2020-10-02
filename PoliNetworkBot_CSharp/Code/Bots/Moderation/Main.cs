#region

using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class Main
    {
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            var t = new Thread(() => _ = MainMethod2(sender, e));
            t.Start();
        }

        private static async Task MainMethod2(object sender, MessageEventArgs e)
        {
            TelegramBotClient telegramBotClientBot = null;
            TelegramBotAbstract telegramBotClient = null;

            try
            {
                if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

                if (telegramBotClientBot == null)
                    return;

                telegramBotClient = TelegramBotAbstract.GetFromRam(telegramBotClientBot);

                var toExit = await ModerationCheck.CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
                if (toExit.Item1 == ToExit.EXIT)
                {
                    string itemToPrint = MemberListToString(toExit.Item2);
                    string itemToPrint2 = ListIntToString(toExit.Item3);
                    string itemToPrint3 = StringToStringToBePrinted(toExit.Item4);
                    string itemToPrintFull = itemToPrint + "\n" + e?.Message?.Chat?.Title;
                    itemToPrintFull += "\n----\n" + itemToPrint2 + "\n----\nS:" + itemToPrint3;
                    itemToPrintFull += "\n----\n" + e?.Message?.Chat?.Id.ToString();
                    itemToPrintFull += "\n@@@@@@";

                    throw new Exception(itemToPrintFull);
                    //await LeaveChat.ExitFromChat(telegramBotClient, e);
                    return;
                }

                List<long> NotAuthorizedBotHasBeenAddedBool = await ModerationCheck.CheckIfNotAuthorizedBotHasBeenAdded(e, telegramBotClient);
                if (NotAuthorizedBotHasBeenAddedBool != null && NotAuthorizedBotHasBeenAddedBool.Count > 0)
                {
                    foreach (var bot in NotAuthorizedBotHasBeenAddedBool)
                    {
                        await Utils.RestrictUser.BanUserFromGroup(telegramBotClient, e, bot, e.Message.Chat.Id, null);
                    }

                    //todo: send messagge "Bots not allowed here!"
                }

                var toExitBecauseUsernameAndNameCheck = await ModerationCheck.CheckUsernameAndName(e, telegramBotClient);
                if (toExitBecauseUsernameAndNameCheck)
                    return;

                var checkSpam = ModerationCheck.CheckSpam(e);
                if (checkSpam != SpamType.ALL_GOOD)
                {
                    await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam);
                    return;
                }

                if (e.Message.Text != null && e.Message.Text.StartsWith("/"))
                    await CommandDispatcher.CommandDispatcherMethod(telegramBotClient, e);
                else
                    await TextConversation.DetectMessage(telegramBotClient, e);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                await Utils.NotifyUtil.NotifyOwners(exception, telegramBotClient);
            }
        }

        private static string StringToStringToBePrinted(string item4)
        {
            if (item4 == null)
                return "[NULL]";

            if (item4.Length == 0)
                return "[EMPTY]";

            return item4;
        }

        private static string ListIntToString(List<int> item3)
        {
            if (item3 == null)
                return "[NULL]";

            if (item3.Count() == 0)
                return "[EMPTY]";

            string r = "";
            foreach (var item4 in item3)
            {
                r += item4 + "\n";
            }
            return r;
        }

        private static string MemberListToString(ChatMember[] item2)
        {
            if (item2 == null)
                return "[NULL]";

            if (item2.Count() == 0)
                return "[EMPTY]";

            string r = "";
            foreach (var item3 in item2)
            {
                r += item3?.User?.Username + " " + item3?.Status + "\n";
            }
            return r;
        }
    }
}