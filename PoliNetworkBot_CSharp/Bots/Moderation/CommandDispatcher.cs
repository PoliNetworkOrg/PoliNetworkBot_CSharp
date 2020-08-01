using System;
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

                case "/help":
                    {
                        Help(sender, e);
                        return;
                    }

                default:
                    {
                        Utils.SendMessage.SendMessageInPrivate(sender, e, "Mi dispiace, ma non conosco questo comando. Prova a contattare gli amministratori (/contact)");
                        return;
                    }
            }
        }

        private static void Help(TelegramBotAbstract sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
            {
                HelpPrivate(sender, e);
            }
            else
            {
                Utils.SendMessage.SendMessageInPrivateOrAGroup(sender, e, "Questo messaggio funziona solo in chat privata");
            }
        }

        private static void HelpPrivate(TelegramBotAbstract sender, MessageEventArgs e)
        {
            string text = "<i>Lista di funzioni</i>:\n"+
                                      "\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n"+
                                      "\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n"+
                                      "\n🙋 <a href='https://polinetwork.github.io/it/faq/index.html'>"+
                                      "FAQ (domande frequenti)</a>\n"+
                                      "\n🏫 Bot ricerca aule libere @AulePolimiBot\n"+
                                      "\n🕶️ Sistema di pubblicazione anonima (per maggiori info /help_anon)\n"+
                                      "\n🎙️ Registrazione delle lezioni (per maggiori info /help_record)\n"+
                                      "\n👥 Gruppo consigliati e utili /groups\n"+
                                      "\n⚠ Hai già letto le regole del network? /rules\n"+
                                      "\n✍ Per contattarci /contact";
            Utils.SendMessage.SendMessageInPrivate(sender, e, text, Telegram.Bot.Types.Enums.ParseMode.Html);
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