#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class CommandDispatcher
    {
        public static async Task CommandDispatcherMethod(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var cmdLines = e.Message.Text.Split(' ');
            var cmd = cmdLines[0];
            switch (cmd)
            {
                case "/start":
                {
                    await Start(sender, e);
                    return;
                }

                case "/force_check_invite_links":
                {
                    if (GlobalVariables.Creators.Contains(e.Message.Chat.Username))
                        _ = ForceCheckInviteLinksAsync(sender, e);
                    else
                        await DefaultCommand(sender, e);
                    return;
                }

                case "/contact":
                {
                    await ContactUs(sender, e);
                    return;
                }

                case "/help":
                {
                    await Help(sender, e);
                    return;
                }

                case "/banAll":
                {
                    if (GlobalVariables.Creators.Contains(e.Message.From.Username))
                        _ = BanAllAsync(sender, e, cmdLines, e.Message.From.LanguageCode, e.Message.From.Username);
                    else
                        await DefaultCommand(sender, e);
                    return;
                }

                case "/ban":
                {
                    _ = BanUserAsync(sender, e, cmdLines);
                    return;
                }

                case "/unbanAll":
                {
                    if (GlobalVariables.Creators.Contains(e.Message.From.Username))
                        _ = UnbanAllAsync(sender, e, cmdLines[1]);
                    else
                        await DefaultCommand(sender, e);
                    return;
                }

                case "/groups":
                {
                    await SendRecommendedGroupsAsync(sender, e);
                    return;
                }

                case "/getGroups":
                {
                    if (GlobalVariables.Creators.Contains(e.Message.From.Username) && e.Message.Chat.Type == ChatType.Private)
                    {
                        string username = null;
                        if (!string.IsNullOrEmpty(e.Message.From.Username)) username = e.Message.From.Username;
                        _ = GetAllGroups(e.Message.From.Id, username, sender, e.Message.From.LanguageCode);
                        return;
                    }

                    await DefaultCommand(sender, e);

                    return;
                }

                case "/time":
                {
                    var lang = new Language(new Dictionary<string, string>
                    {
                        {"", DateTimeClass.NowAsStringAmericanFormat()}
                    });
                    await SendMessage.SendMessageInPrivate(sender, userIdToSendTo: e.Message.From.Id,
                        usernameToSendTo: e.Message.From.Username,  langCode: e.Message.From.LanguageCode, text: lang);
                    return;
                }

                case "/assoc_send":
                {
                    _ = await Assoc.Assoc_SendAsync(sender, e);
                    return;
                }

                case "/rooms":
                    {
                        await Bots.Moderation.Rooms.RoomsMainAsync(sender, e);
                        return;
                    }

                default:
                {
                    await DefaultCommand(sender, e);
                    return;
                }
            }
        }

        private static async Task SendRecommendedGroupsAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string text = "<i>Lista di gruppi consigliati</i>:\n" +
                "\n👥 Gruppo di tutti gli studenti @PoliGruppo 👈\n" +
                "\n📖 Libri @PoliBook\n" +
                "\n🤪 Spotted & Memes @PolimiSpotted @PolimiMemes\n" +
                "\n🥳 Eventi @PoliEventi\n" +
                "\nRicordiamo che sul nostro sito vi sono tutti i link ai gruppi con tanto ricerca, facci un salto!\n" +
                "https://polinetwork.github.io/" ;


            const string textEng = "<i>List of recommended groups</i>:\n" +
                "\n👥 Group with all students @PoliGruppo 👈\n" +
                "\n📖 Books @PoliBook\n" +
                "\n🤪 Spotted & Memes @PolimiSpotted @PolimiMemes\n" +
                "\n🥳 Events @PoliEventi\n" +
                "\nWe remind you that on our website there are all link to the groups, and they are searchable, have a look!\n" +
                "https://polinetwork.github.io/";


            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", textEng},
                {"it", text}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
e.Message.From.LanguageCode,
e.Message.From.Username, text2, ParseMode.Html);
        }

        public static async Task<bool> GetAllGroups(long chatId, string username, TelegramBotAbstract sender,
            string lang)
        {
            var groups = Groups.GetAllGroups();
            Stream stream = new MemoryStream();
            FileSerialization.SerializeFile(groups, ref stream);
            TLAbsInputPeer peer2 = new TLInputPeerUser {UserId = (int) chatId};
            var peer = new Tuple<TLAbsInputPeer, long>(peer2, chatId);


            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", "Here are all groups:"},
                {"it", "Ecco tutti i gruppi:"}
            });
            return await SendMessage.SendFileAsync(new TelegramFile(stream, "groups.bin",
                    null, "application/octet-stream"), peer,
                text2, TextAsCaption.BEFORE_FILE,
                sender, username, lang);
        }

        private static async Task<bool> BanUserAsync(TelegramBotAbstract sender, MessageEventArgs e,
            string[] stringInfo)
        {
            var r = await Groups.CheckIfAdminAsync(e.Message.From.Id, e.Message.From.Username, e.Message.Chat.Id, sender);
            if (!r)
                return false;

            if (e.Message.ReplyToMessage == null)
            {
                var targetInt = await Info.GetTargetUserIdAsync(stringInfo[1], sender);
                return targetInt != null &&
                       await RestrictUser.BanUserFromGroup(sender, e, targetInt.Value, e.Message.Chat.Id, null);
            }
            else
            {
                var targetInt = e.Message.ReplyToMessage.From.Id;
                return await RestrictUser.BanUserFromGroup(sender, e, targetInt, e.Message.Chat.Id, stringInfo);
            }
        }

        private static async Task UnbanAllAsync(TelegramBotAbstract sender, MessageEventArgs e, string target)
        {
            var done = await RestrictUser.BanAllAsync(sender, e, target, false);
            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", "Target unbanned from " + done.Count + " groups"},
                {"it", "Target sbannato da " + done.Count + " gruppi"}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
e.Message.From.LanguageCode,
e.Message.From.Username, text2);
        }

        private static async Task BanAllAsync(TelegramBotAbstract sender, MessageEventArgs e,
            IReadOnlyList<string> target, string lang, string username)
        {
            if (e.Message.ReplyToMessage == null)
            {
                if (target.Count < 2)
                {
                    var lang2 = new Language(new Dictionary<string, string>
                    {
                        {"en", "We can't find the target."},
                        {"it", "Non riusciamo a trovare il bersaglio"}
                    });
                    await sender.SendTextMessageAsync(e.Message.From.Id, lang2, ChatType.Private,
                        lang, ParseMode.Default, username: username,
                        replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE));
                }
                else
                {
                    var done = await RestrictUser.BanAllAsync(sender, e, target[1], true);
                    var text2 = new Language(new Dictionary<string, string>
                    {
                        {"en", "Target banned from " + done.Count + " groups"},
                        {"it", "Target bannato da " + done.Count + " gruppi"}
                    });
                    await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
e.Message.From.LanguageCode,
e.Message.From.Username, text2);
                }
            }
            else
            {
                var done = await RestrictUser.BanAllAsync(sender, e, e.Message.ReplyToMessage.From.Id.ToString(), true);
                var text3 = new Language(new Dictionary<string, string>
                {
                    {"en", "Target banned from " + done.Count + " groups"},
                    {"it", "Target bannato da " + done.Count + " gruppi"}
                });
                await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
e.Message.From.LanguageCode,
e.Message.From.Username, text3);
            }
        }

        private static async Task DefaultCommand(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var text2 = new Language(new Dictionary<string, string>
            {
                {
                    "en",
                    "I'm sorry, but I don't know this command. Try to ask the administrators (/contact)"
                },
                {
                    "it",
                    "Mi dispiace, ma non conosco questo comando. Prova a contattare gli amministratori (/contact)"
                }
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
e.Message.From.LanguageCode,
e.Message.From.Username, text2);
        }

        private static async Task Help(TelegramBotAbstract sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == ChatType.Private)
            {
                await HelpPrivate(sender, e);
            }
            else
            {
                var lang = new Language(new Dictionary<string, string>
                {
                    {"it", "Questo messaggio funziona solo in chat privata"},
                    {"en", "This command only works in private chat with me"}
                });
                await SendMessage.SendMessageInPrivateOrAGroup(sender,
                    lang, e.Message.From.LanguageCode, e.Message.From.Username, e.Message.From.Id,
                    e.Message.From.FirstName, e.Message.From.LastName, e.Message.Chat.Id, e.Message.Chat.Type);
            }
        }

        private static async Task HelpPrivate(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string text = "<i>Lista di funzioni</i>:\n" +
                                //"\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n" +
                                //"\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n" +
                                "\n🙋 <a href='https://polinetwork.github.io/it/faq/index.html'>" +
                                "FAQ (domande frequenti)</a>\n" +
                                "\n🏫 Ricerca aule libere /rooms\n" +
                                //"\n🕶️ Sistema di pubblicazione anonima (per maggiori info /help_anon)\n" +
                                //"\n🎙️ Registrazione delle lezioni (per maggiori info /help_record)\n" +
                                "\n👥 Gruppo consigliati e utili /groups\n" +
                                "\n⚠ Hai già letto le regole del network? /rules\n" +
                                "\n✍ Per contattarci /contact";


            const string textEng = "<i>List of features</i>:\n" +
                                   //"\n📑 Review system of courses (for more info /help_review)\n" +
                                   //"\n🔖 Link to notes (for more info /help_material)\n" +
                                   "\n🙋 <a href='https://polinetwork.github.io/it/faq/index.html'>" +
                                   "FAQ (frequently asked questions)</a>\n" +
                                   "\n🏫 Find free rooms /rooms\n" +
                                   //"\n🕶️ Anonymous posting system (for more info /help_anon)\n" +
                                   //"\n🎙️ Record of lessons (for more info /help_record)\n" +
                                   "\n👥 Recommended groups /groups\n" +
                                   "\n⚠ Have you already read our network rules? /rules\n" +
                                   "\n✍ To contact us /contact";


            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", textEng},
                {"it", text}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
e.Message.From.LanguageCode,
e.Message.From.Username, text2, ParseMode.Html);
        }

        private static async Task ContactUs(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            var lang2 = new Language(new Dictionary<string, string>
            {
                {"it", telegramBotClient.GetContactString()},
                {"en", telegramBotClient.GetContactString()}
            });
            await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                lang2, e.Message.Chat.Type, e.Message.From.LanguageCode,
                ParseMode.Default,
                new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username
            );
        }

        private static async Task ForceCheckInviteLinksAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var n = await InviteLinks.FillMissingLinksIntoDB_Async(sender);
            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", "I have updated n=" + n + " links"},
                {"it", "Ho aggiornato n=" + n + " link"}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
e.Message.From.LanguageCode,
e.Message.From.Username, text2);
        }

        private static async Task Start(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            var lang2 = new Language(new Dictionary<string, string>
            {
                {
                    "it", "Ciao! 👋\n" +
                          "\nScrivi /help per la lista completa delle mie funzioni 👀\n" +
                          "\nVisita anche il nostro sito " + telegramBotClient.GetWebSite()
                },
                {
                    "en", "Hi! 👋\n" +
                          "\nWrite /help for the complete list of my functions👀\n" +
                          "\nAlso visit our site " + telegramBotClient.GetWebSite()
                }
            });
            await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                lang2,
                e.Message.Chat.Type, replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                lang: e.Message.From.LanguageCode, username: e.Message.From.Username, parseMode: ParseMode.Default
            );
        }
    }
}