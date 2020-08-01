using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Bots.Moderation
{
    internal class CommandDispatcher
    {
        public static void CommandDispatcherMethod(TelegramBotAbstract sender, MessageEventArgs e)
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

                case "/contact":
                    {
                        ContactUs(sender, e);
                        return;
                    }

                default:
                    {
                        Utils.SendMessage.SendMessageInPrivate(sender, e, "Mi dispiace, ma non conosco questo comando. Prova a contattare gli amministratori (/contact)");
                        return;
                    }
            }
        }

        private static void ContactUs(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            Utils.DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                    telegramBotClient.GetContactString()
                );
        }

        private static async System.Threading.Tasks.Task ForceCheckInviteLinksAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            int n = await Utils.InviteLinks.FillMissingLinksIntoDB_Async(sender);
            Utils.SendMessage.SendMessageInPrivate(sender, e, "I have updated n=" + n.ToString() + " links");
        }

        private static void Start(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            Utils.DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                    "Ciao! 👋\n" +
                    "\nScrivi /help per la lista completa delle mie funzioni 👀\n" +
                    "\nVisita anche il nostro sito " + telegramBotClient.GetWebSite()
                );
        }
    }
}