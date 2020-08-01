using Telegram.Bot;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Bots.Moderation
{
    internal class CommandDispatcher
    {
        public static void CommandDispatcherMethod(TelegramBotClient sender, MessageEventArgs e)
        {
            switch (e.Message.Text)
            {
                case "/start":
                    {
                        Start(sender, e);
                        return;
                    }

                case "/force_check_invite_links":
                    {
                        if (Data.GlobalVariables.Creators.Contains(e.Message.Chat.Id))
                        {
                            _ = ForceCheckInviteLinksAsync(sender, e);
                        }
                        return;
                    }
            }
        }

        private static async System.Threading.Tasks.Task ForceCheckInviteLinksAsync(TelegramBotClient sender, MessageEventArgs e)
        {
            int n = await Utils.InviteLinks.FillMissingLinksIntoDB_Async(sender);
            Utils.SendMessage.SendMessageInPrivate(sender, e, "I have updated n=" + n.ToString() + " links");
        }

        private static void Start(TelegramBotClient telegramBotClient, MessageEventArgs e)
        {
            if (e.Message.Chat.Type != Telegram.Bot.Types.Enums.ChatType.Private)
            {
                try
                {
                    telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);
                }
                catch
                {
                    ;
                }
            }
            telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id, "Ciao!");
        }
    }
}