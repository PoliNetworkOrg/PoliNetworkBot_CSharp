using PoliNetworkBot_CSharp.Data;
using PoliNetworkBot_CSharp.Utils;
using System.IO;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Bots.Moderation
{
    internal class CommandDispatcher
    {
        public static void CommandDispatcherMethod(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var cmd_lines = e.Message.Text.Split(' ');
            string cmd = cmd_lines[0];
            switch (cmd)
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
                        else
                        {
                            DefaultCommand(sender, e);
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

                case "/banAll":
                    {
                        if (GlobalVariables.Creators.Contains(e.Message.From.Id))
                        {
                            var done = RestrictUser.BanAll(sender, e, cmd_lines[1], true);
                            Utils.SendMessage.SendMessageInPrivate(sender, e,
                                "Target banned from " + done.Count.ToString() + " groups");
                        }
                        else
                        {
                            DefaultCommand(sender, e);
                        }
                        return;
                    }

                case "/unbanAll":
                    {
                        if (GlobalVariables.Creators.Contains(e.Message.From.Id))
                        {
                            var done = RestrictUser.BanAll(sender, e, cmd_lines[1], false);
                            Utils.SendMessage.SendMessageInPrivate(sender, e,
                                "Target unbanned from " + done.Count.ToString() + " groups");
                        }
                        else
                        {
                            DefaultCommand(sender, e);
                        }
                        return;
                    }

                case "/getGroups":
                    {
                        if (GlobalVariables.Creators.Contains(e.Message.From.Id) && e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
                        {
                            System.Data.DataTable groups = Utils.Groups.GetAllGroups();
                            Stream stream = new MemoryStream();
                            Utils.FileSerialization.SerializeFile(groups, ref stream);
                            _ = SendMessage.SendFileAsync(File: new TelegramFile(stream, "groups.bin"), chat_id: e.Message.Chat.Id,
                                text: "Here are all groups:", text_as_caption: Enums.TextAsCaption.BEFORE_FILE,
                                TelegramBot_Abstract: sender);
                        }
                        else
                        {
                            DefaultCommand(sender, e);
                        }
                        return;
                    }

                default:
                    {
                        DefaultCommand(sender, e);
                        return;
                    }
            }
        }

        private static void DefaultCommand(TelegramBotAbstract sender, MessageEventArgs e)
        {
            Utils.SendMessage.SendMessageInPrivate(sender, e, "Mi dispiace, ma non conosco questo comando. Prova a contattare gli amministratori (/contact)");
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
            string text = "<i>Lista di funzioni</i>:\n" +
                                      "\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n" +
                                      "\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n" +
                                      "\n🙋 <a href='https://polinetwork.github.io/it/faq/index.html'>" +
                                      "FAQ (domande frequenti)</a>\n" +
                                      "\n🏫 Bot ricerca aule libere @AulePolimiBot\n" +
                                      "\n🕶️ Sistema di pubblicazione anonima (per maggiori info /help_anon)\n" +
                                      "\n🎙️ Registrazione delle lezioni (per maggiori info /help_record)\n" +
                                      "\n👥 Gruppo consigliati e utili /groups\n" +
                                      "\n⚠ Hai già letto le regole del network? /rules\n" +
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