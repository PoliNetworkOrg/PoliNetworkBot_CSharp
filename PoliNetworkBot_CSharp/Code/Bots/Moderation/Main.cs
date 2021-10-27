#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class Main
    {
        private static TelegramBotClient _telegramBotClientBot = null;
        private static TelegramBotAbstract _telegramBotClient = null;
        static AutoResetEvent autoEvent = new AutoResetEvent(false);
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            var t = new Thread(() => _ = MainMethod2(sender, e));
            t.Start();
            var t1 = new Thread(() => _ = CheckAllowedMessageExpiration(sender, e));
            t1.Start();
            var t2 = new Thread(() => _ = BackupHandler(Data.Constants.Groups.BackupGroup, _telegramBotClient, null));
            t2.Start();
        }

        private static async Task BackupHandler(long backupGroup, TelegramBotAbstract telegramBotClient, object o)
        {
            autoEvent.WaitOne();
            while (true)
            {
                await CommandDispatcher.BackupHandler(Data.Constants.Groups.BackupGroup, _telegramBotClient, null);
                Thread.Sleep(1000 * 3600 * 24 * 7);
            }
        }

        private static object CheckAllowedMessageExpiration(object sender, MessageEventArgs messageEventArgs)
        {
            while (true)
            {
                AllowedMessages.CheckTimestamp();
                Thread.Sleep(1000 * 3600 * 24);
            }
        }

        private static async Task MainMethod2(object sender, MessageEventArgs e)
        {

            try
            {
                if (sender is TelegramBotClient tmp) _telegramBotClientBot = tmp;

                if (_telegramBotClientBot == null)
                    return;

                _telegramBotClient = TelegramBotAbstract.GetFromRam(_telegramBotClientBot);

                autoEvent.Set();

                var toExit = await ModerationCheck.CheckIfToExitAndUpdateGroupList(_telegramBotClient, e);
                if (toExit.Item1 == ToExit.EXIT)
                {
                    var itemToPrint = MemberListToString(toExit.Item2);
                    var itemToPrint2 = ListIntToString(toExit.Item3);
                    var itemToPrint3 = StringToStringToBePrinted(toExit.Item4);
                    var itemToPrintFull = itemToPrint + "\n" + e?.Message?.Chat?.Title;
                    itemToPrintFull += "\n----\n" + itemToPrint2 + "\n----\nS:" + itemToPrint3;
                    itemToPrintFull += "\n----\n" + e?.Message?.Chat?.Id;
                    itemToPrintFull += "\n@@@@@@";

                    throw new ToExitException(itemToPrintFull);
                }

                var notAuthorizedBotHasBeenAddedBool =
                    await ModerationCheck.CheckIfNotAuthorizedBotHasBeenAdded(e, _telegramBotClient);
                if (notAuthorizedBotHasBeenAddedBool != null && notAuthorizedBotHasBeenAddedBool.Count > 0)
                    foreach (var bot in notAuthorizedBotHasBeenAddedBool)
                        await RestrictUser.BanUserFromGroup(_telegramBotClient, e, bot, e.Message.Chat.Id, null, true);

                //todo: send messagge "Bots not allowed here!"

                if (banMessageDetected(e))
                {
                    CommandDispatcher.BanMessageActions(_telegramBotClient, e);
                    return;
                }

                var toExitBecauseUsernameAndNameCheck =
                    await ModerationCheck.CheckUsernameAndName(e, _telegramBotClient);
                if (toExitBecauseUsernameAndNameCheck)
                    return;

                var checkSpam = ModerationCheck.CheckSpam(e);
                if (checkSpam != SpamType.ALL_GOOD && checkSpam != SpamType.SPAM_PERMITTED)
                {
                    await ModerationCheck.AntiSpamMeasure(_telegramBotClient, e, checkSpam);
                    return;
                }

                if (checkSpam == SpamType.SPAM_PERMITTED)
                {
                    await ModerationCheck.PermittedSpamMeasure(_telegramBotClient, e, checkSpam);
                    return;
                }

                if (e.Message.Text != null && e.Message.Text.StartsWith("/"))
                    await CommandDispatcher.CommandDispatcherMethod(_telegramBotClient, e);
                else
                    await TextConversation.DetectMessage(_telegramBotClient, e);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                await NotifyUtil.NotifyOwners(exception, _telegramBotClient);
            }
        }

        private static bool banMessageDetected(MessageEventArgs messageEventArgs)
        {
            return false; //todo
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

            var r = "";
            foreach (var item4 in item3) r += item4 + "\n";
            return r;
        }

        private static string MemberListToString(ChatMember[] item2)
        {
            if (item2 == null)
                return "[NULL]";

            if (item2.Count() == 0)
                return "[EMPTY]";

            var r = "";
            foreach (var item3 in item2) r += item3?.User?.Username + " " + item3?.Status + "\n";
            return r;
        }
    }
}