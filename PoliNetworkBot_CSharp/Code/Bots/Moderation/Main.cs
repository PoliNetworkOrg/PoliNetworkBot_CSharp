#region

using System.Threading;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal class Main
    {
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            var t = new Thread(() => MainMethod2(sender, e));
            t.Start();
        }

        private static void MainMethod2(object sender, MessageEventArgs e)
        {
            TelegramBotClient telegramBotClient_bot = null;
            if (sender is TelegramBotClient tmp) telegramBotClient_bot = tmp;

            if (telegramBotClient_bot == null)
                return;

            var telegramBotClient = TelegramBotAbstract.GetFromRam(telegramBotClient_bot);

            var to_exit = ModerationCheck.CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
            if (to_exit)
            {
                LeaveChat.ExitFromChat(telegramBotClient, e);
                return;
            }

            var check_username = ModerationCheck.CheckUsername(e);
            if (check_username.Item1 || check_username.Item2)
            {
                ModerationCheck.SendUsernameWarning(telegramBotClient, e, check_username.Item1, check_username.Item2);
                return;
            }

            var check_spam = ModerationCheck.CheckSpam(e);
            if (check_spam != SpamType.ALL_GOOD)
            {
                ModerationCheck.AntiSpamMeasure(telegramBotClient, e, check_spam);
                return;
            }

            if (!string.IsNullOrEmpty(e.Message.Text))
            {
                if (e.Message.Text.StartsWith("/"))
                    CommandDispatcher.CommandDispatcherMethod(telegramBotClient, e);
                else
                    TextConversation.DetectMessage(telegramBotClient, e);
            }
        }
    }
}