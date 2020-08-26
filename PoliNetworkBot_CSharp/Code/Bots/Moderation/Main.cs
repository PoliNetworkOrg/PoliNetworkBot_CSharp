#region

using System;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot;
using Telegram.Bot.Args;

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
            if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

            if (telegramBotClientBot == null)
                return;

            var telegramBotClient = TelegramBotAbstract.GetFromRam(telegramBotClientBot);

            var toExit = await ModerationCheck.CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
            if (toExit)
            {
                await LeaveChat.ExitFromChat(telegramBotClient, e);
                return;
            }

            var (item1, item2) = ModerationCheck.CheckUsername(e);
            if (item1 || item2)
            {
                await ModerationCheck.SendUsernameWarning(telegramBotClient, e, item1, 
                    item2, lang:e.Message.From.LanguageCode, e.Message.From.Username);
                return;
            }

            var checkSpam = ModerationCheck.CheckSpam(e);
            if (checkSpam != SpamType.ALL_GOOD)
            {
                await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam);
                return;
            }

            if (string.IsNullOrEmpty(e.Message.Text))
                return;

            if (e.Message.Text.StartsWith("/"))
                await CommandDispatcher.CommandDispatcherMethod(telegramBotClient, e);
            else
                TextConversation.DetectMessage(telegramBotClient, e);
        }
    }
}