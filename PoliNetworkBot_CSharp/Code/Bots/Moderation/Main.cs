#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
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
            //var t1 = new Thread(() => _ = CheckAllowedMessageExpiration(sender, e));
            //t1.Start();
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
                    var itemToPrint = MemberListToString(toExit.Item2);
                    var itemToPrint2 = ListIntToString(toExit.Item3);
                    var itemToPrint3 = StringToStringToBePrinted(toExit.Item4);
                    var itemToPrintFull = itemToPrint + "\n" + e?.Message?.Chat?.Title;
                    itemToPrintFull += "\n----\n" + itemToPrint2 + "\n----\nS:" + itemToPrint3;
                    itemToPrintFull += "\n----\n" + e?.Message?.Chat?.Id;
                    itemToPrintFull += "\n@@@@@@";

                    await Utils.Groups.SendMessageExitingAndThenExit(telegramBotClient, e);

                    throw new ToExitException(itemToPrintFull);
                }

                await Groups.CheckForGroupUpdateAsync(telegramBotClient, e);

                var notAuthorizedBotHasBeenAddedBool =
                    await ModerationCheck.CheckIfNotAuthorizedBotHasBeenAdded(e, telegramBotClient);
                if (notAuthorizedBotHasBeenAddedBool != null && notAuthorizedBotHasBeenAddedBool.Count > 0)
                    foreach (var bot in notAuthorizedBotHasBeenAddedBool)
                        await RestrictUser.BanUserFromGroup(telegramBotClient, bot, e.Message.Chat.Id, null, true);

                //todo: send messagge "Bots not allowed here!"

                //if (BanMessageDetected(e)) todo:BanMessageDetected
                //{
                //   await CommandDispatcher.BanMessageActions(telegramBotClient, e);
                //    return;
                //}

                var toExitBecauseUsernameAndNameCheck =
                    await ModerationCheck.CheckUsernameAndName(e, telegramBotClient);
                if (toExitBecauseUsernameAndNameCheck)
                    return;

                var checkSpam = ModerationCheck.CheckSpam(e);
                if (checkSpam != SpamType.ALL_GOOD && checkSpam != SpamType.SPAM_PERMITTED)
                {
                    await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam);
                    return;
                }

                if (checkSpam == SpamType.SPAM_PERMITTED)
                {
                    await ModerationCheck.PermittedSpamMeasure(telegramBotClient, e, checkSpam);
                    return;
                }

                if (e.Message.Text != null && e.Message.Text.StartsWith("/"))
                    await CommandDispatcher.CommandDispatcherMethod(telegramBotClient, e);
                else
                    await TextConversation.DetectMessage(telegramBotClient, e);
            }
            catch (Exception exception)
            {
                Logger.WriteLine(exception.Message);

                await NotifyUtil.NotifyOwners(exception, telegramBotClient);
            }
        }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private static bool BanMessageDetected(MessageEventArgs messageEventArgs)
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        {
            throw new NotImplementedException();
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

            if (item3.Count == 0)
                return "[EMPTY]";

            var r = "";
            foreach (var item4 in item3) r += item4 + "\n";
            return r;
        }

        private static string MemberListToString(ChatMember[] item2)
        {
            if (item2 == null)
                return "[NULL]";

            if (item2.Length == 0)
                return "[EMPTY]";

            var r = "";
            foreach (var item3 in item2) r += item3?.User?.Username + " " + item3?.Status + "\n";
            return r;
        }
    }
}