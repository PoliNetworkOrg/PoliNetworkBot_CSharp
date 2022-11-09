using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation;

public static class Help
{
    public static async Task HelpSpecific(MessageEventArgs? e, TelegramBotAbstract? sender, string[] args)
    {
        var command = CommandDispatcher.Commands.Find(x => x.GetTriggers().Contains(args[0]));

        if (command == null)
            return;

        var text2 = new Language(new Dictionary<string, string?>
        {
            {
                "en",
                "\n<b>Command description:</b>\n" +
                command.GetLongDescription(Permissions.GetPrivileges(e?.Message.From)).Select("en")
            },
            {
                "it",
                "\n<b>Descrizione del comando:</b>\n" +
                command.GetLongDescription(Permissions.GetPrivileges(e?.Message.From)).Select("it")
            }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(),
            EventArgsContainer.Get(e));
    }

    public static async Task HelpExtendedSlave(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        const string text = "<i>Lista di funzioni</i>:\n" +
                            //"\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n" +
                            //"\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n" +
                            "\n🙋 <a href='https://polinetwork.org/it/faq/index.html'>" +
                            "FAQ (domande frequenti)</a>\n\n";
        //"\n🕶️ Sistema di pubblicazione anonima (per maggiori info /help_anon)\n" +
        //"\n🎙️ Registrazione delle lezioni (per maggiori info /help_record)\n" +
        const string textEng = "<i>List of features</i>:\n" +
                               //"\n📑 Review system of courses (for more info /help_review)\n" +
                               //"\n🔖 Link to notes (for more info /help_material)\n" +
                               "\n🙋 <a href='https://polinetwork.org/it/faq/index.html'>" +
                               "FAQ (frequently asked questions)</a>\n\n";


        var text2 = new Language(new Dictionary<string, string?>
        {
            {
                "en",
                textEng + "\n<b>Commands available:</b>\n" +
                string.Join("",
                    CommandDispatcher.Commands.Select(x =>
                        x.HelpMessage(Permissions.GetPrivileges(e?.Message.From)).Select("en")))
            },
            {
                "it",
                text + "\n<b>Comandi disponibili:</b>\n" +
                string.Join("",
                    CommandDispatcher.Commands.Select(x =>
                        x.HelpMessage(Permissions.GetPrivileges(e?.Message.From)).Select("it")))
            }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(),
            EventArgsContainer.Get(e));
    }

    public static async Task HelpPrivateSlave(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        const string text = "<i>Lista di funzioni</i>:\n" +
                            //"\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n" +
                            //"\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n" +
                            "\n🙋 <a href='https://polinetwork.org/it/faq/index.html'>" +
                            "FAQ (domande frequenti)</a>\n" +
                            "\nLista di tutti i comandi /help_all\n" +
                            "\n🏫 Ricerca aule libere /rooms\n" +
                            //"\n🕶️ Sistema di pubblicazione anonima (per maggiori info /help_anon)\n" +
                            //"\n🎙️ Registrazione delle lezioni (per maggiori info /help_record)\n" +
                            "\n👥 Gruppo consigliati e utili /groups\n" +
                            "\n⚠ Hai già letto le regole del network? /rules\n" +
                            "\n✍ Per contattarci /contact";

        const string textEng = "<i>List of features</i>:\n" +
                               //"\n📑 Review system of courses (for more info /help_review)\n" +
                               //"\n🔖 Link to notes (for more info /help_material)\n" +
                               "\n🙋 <a href='https://polinetwork.org/it/faq/index.html'>" +
                               "FAQ (frequently asked questions)</a>\n" +
                               "\nAll available commands /help_all\n" +
                               "\n🏫 Find free rooms /rooms\n" +
                               //"\n🕶️ Anonymous posting system (for more info /help_anon)\n" +
                               //"\n🎙️ Record of lessons (for more info /help_record)\n" +
                               "\n👥 Recommended groups /groups\n" +
                               "\n⚠ Have you already read our network rules? /rules\n" +
                               "\n✍ To contact us /contact";

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", textEng },
            { "it", text }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message?.From?.Id,
            e?.Message?.From?.LanguageCode,
            e?.Message?.From?.Username, text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(),
            EventArgsContainer.Get(e));
    }
}