#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;
using Groups = PoliNetworkBot_CSharp.Code.Data.Constants.GroupsConstants;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Conversation;

public static class AutoReplyInGroups
{
    private const bool AreWhatsappLinksPublic = false;
    private static readonly DateTime DsuLimit = new(2022, 11, 30);

    private static readonly List<AutomaticAnswer> AutomaticAnswers = new()
    {
        new AutomaticAnswer(new List<List<string>>
            {
                new() { "piano studi", "piano di studi", "piano degli studi" }
            }, Reply,
            new List<long> { Groups.PianoDiStudi, Groups.AskPolimi },
            "Ciao 👋 sembra tu stia facendo domande in merito al piano di studi. " +
            "PoliNetwork ti consiglia di scrivere nel gruppo dedicato, " +
            "<a href='https://t.me/joinchat/aiAC6RgOjBRkYjhk'>clicca qui</a>!"),

        new AutomaticAnswer(new List<List<string>>
            {
                new() { "study plan" }
            }, Reply,
            new List<long> { Groups.PianoDiStudi, Groups.AskPolimi },
            "Hi 👋 it seems you are asking questions about the study plan. " +
            "PoliNetwork advices you to write in the dedicated group, " +
            "<a href='https://t.me/joinchat/aiAC6RgOjBRkYjhk'>click here</a>!"),

        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() { "piano" },
                new() { "attesa", "non" },
                new() { "approva" }
            }, Reply,
            new List<long>(),
            "Se non avete fatto cose strane tipo: inserito esami autonomi senza essersi accertati preventivamente che fossero ok, " +
            "vincoli del regolamento non rispettati ecc, non vi preoccupate. Il piano prima o poi vi verrà approvato. " +
            "In caso abbiate presentato un piano personalizzato sarà la commissione stessa a contattarvi in caso di problemi",
            e => e?.Message.Chat.Id == Groups.PianoDiStudi),

        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() { "quando" },
                new() { "presentare", "compilare", "modificare" }
            }, Reply,
            new List<long>(),
            "Ciao, forse la risposta è <a href='https://faq.polinetwork.org/?id=2&cat=1'>qui</a>!",
            e => e?.Message.Chat.Id == Groups.PianoDiStudi),

        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() { "quando" },
                new() { "idone" },
                new() { "beneficiari" }
            }, Reply,
            new List<long>(),
            "Per sapere se idoneo = beneficiario si dovrà attendere il CdA di novembre. " +
            "Finché non viene stabilita la graduatoria definitiva è difficile " +
            "sapere quanti soldi serviranno per attuare la manovra",
            e => e?.Message.Chat.Id == Groups.PianoDiStudi && DateTime.Now <= DsuLimit),

        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() { "when" },
                new() { "eligible" },
                new() { "beneficiary" }
            }, Reply,
            new List<long>(),
            "To find out if eligible = beneficiary you will have to wait for the November Board of Directors. " +
            "Until the final ranking is established, it is difficult " +
            "to know how much money will be needed to implement the policy",
            e => e?.Message.Chat.Id == Groups.PianoDiStudi && DateTime.Now <= DsuLimit),

        new AutomaticAnswer(new List<List<string>>
            {
                new() { "rappresentant", "rappresentanza" }
            }, Reply,
            new List<long> { Groups.AskPolimi },
            "Ciao 👋 sembra tu stia facendo domande in merito alla Rappresentanza. " +
            "PoliNetwork ti consiglia di scrivere nel gruppo dedicato, " +
            "<a href='https://t.me/askPolimi'>clicca qui</a>!"),

        new AutomaticAnswer(new List<List<string>>
            {
                new() { "representative" }
            }, Reply,
            new List<long> { Groups.AskPolimi },
            "Hi 👋 it seems you are asking questions about Representatives. " +
            "PoliNetwork advices you to write in the dedicated group, " +
            "<a href='https://t.me/askPolimi'>click here</a>!"),

        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() { "diritto studio universitario", "diritto allo studio", "dsu" }
            }, Reply,
            new List<long> { Groups.DSU, Groups.AskPolimi },
            "Ciao 👋 sembra tu stia facendo domande in merito al DSU. " +
            "PoliNetwork ti consiglia di scrivere nel gruppo dedicato, " +
            "<a href='https://t.me/joinchat/4kO9DtAiTVM0NTU0'>clicca qui</a>!",
            e => e?.Message.From?.LanguageCode == "it"),

        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() { "scholarship", "dsu" }
            }, Reply,
            new List<long> { Groups.DSU, Groups.AskPolimi },
            "Hi 👋 it seems you are asking questions about 'DSU'. " +
            "PoliNetwork advices you to write in the dedicated group, " +
            "<a href='https://t.me/joinchat/4kO9DtAiTVM0NTU0'>click here</a>!",
            e => e?.Message.From?.LanguageCode != "it"),

        new AutomaticAnswer(new List<List<string>>
            {
                new() { "cento", "100" },
                new() { "maturità" },
                new() { "esonero", "sconto", "tasse" }
            }, Reply,
            new List<long> { Groups.AskPolimi },
            "No."),

        new AutomaticAnswer(new List<List<string>>
            {
                new() { "qualcun", "sa", "notiz" },
                new() { "lezion" },
                new() { "online", "registrazion", "streaming" }
            }, Reply,
            new List<long> { Groups.AskPolimi },
            "Ciao, forse la risposta è <a href='https://faq.polinetwork.org/?id=1&lang=it&cat=3'>qui</a>!"),

        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() { "whatsapp" },
                new() { "grupp" }
            }, Reply,
            new List<long>(),
            "Controlla i messaggi fissati",
            e => e?.Message.Chat.Title != null && e.Message.Chat.Title.ToLower().Contains("matricole") &&
                 AreWhatsappLinksPublic),

        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() { "whatsapp" },
                new() { "group" }
            }, Reply,
            new List<long>(),
            "Check the pinned messages",
            e => e?.Message.Chat.Title != null && e.Message.Chat.Title.ToLower().Contains("matricole") &&
                 AreWhatsappLinksPublic),

        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() { "whatsapp" },
                new() { "grupp" }
            }, Reply,
            new List<long>(),
            "Ciao 👋 sembra tu stia facendo domande in merito ai gruppi Whatsapp. " +
            "Se non l'hai ancora fatto, leggi la guida in merito, " +
            "<a href='https://docs.polinetwork.org/#/it/about/groups/whatsapp'>clicca qui</a>!",
            e => e?.Message.Chat.Title != null && e.Message.Chat.Title.ToLower().Contains("matricole") &&
                 !AreWhatsappLinksPublic),

        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() { "whatsapp" },
                new() { "group" }
            }, Reply,
            new List<long>(),
            "Hi 👋 it seems you are asking questions about Whatsapp groups. " +
            "If you haven't already, we advice you to read the relative guide, " +
            "<a href='https://docs.polinetwork.org/#/en/about/groups/whatsapp'>click here</a>!",
            e => e?.Message.Chat.Title != null && e.Message.Chat.Title.ToLower().Contains("matricole") &&
                 !AreWhatsappLinksPublic),

        new AutomaticAnswer(new List<List<string>>
            {
                new() { "esiste un gruppo" }
            }, Reply,
            new List<long>(),
            "Ciao 👋 sembra tu stia facendo domande in merito ai gruppi. " +
            "PoliNetwork ti consiglia di visitare la sezione gruppi del nostro sito, " +
            "<a href='https://polinetwork.org/groups/'>clicca qui</a>!"),

        new AutomaticAnswer(new List<List<string>>
            {
                new() { "is there a group" }
            }, Reply,
            new List<long>(),
            "Hi 👋 it seems you are asking questions about groups. " +
            "PoliNetwork advices you to visit the groups section of our website, " +
            "<a href='https://polinetwork.org/groups/'>click here</a>!"),

        new AutomaticAnswer(new List<List<string>>
            {
                new() { "graduatori" },
                new() { "qualcun", "puntegg" }
            }, Reply,
            new List<long> { Groups.AskPolimi },
            "Ciao 👋 sembra tu stia facendo domande in merito alle graduatorie degli anni passati. " +
            "PoliNetwork ti consiglia di visitare la sezione del nostro sito dove vengono conservate tutte, " +
            "<a href='https://rankings.polinetwork.org/'>clicca qui</a>!"),

        new AutomaticAnswer(new List<List<string>>
            {
                new() { "ranking" },
                new() { "someone", "score" }
            }, Reply,
            new List<long> { Groups.AskPolimi },
            "Hi 👋 it seems you are asking questions about the past years' rankings. " +
            "PoliNetwork advices you to visit the section of our website where we keep them all, " +
            "<a href='https://rankings.polinetwork.org/'>click here</a>!")
    };

    private static async Task Reply(MessageEventArgs? e, TelegramBotAbstract? telegramBotClient, string message)
    {
        var text = new Language(new Dictionary<string, string?>
        {
            {
                "uni",
                message
            }
        });
        if (e != null)
            await SendMessage.SendMessageInAGroup(telegramBotClient,
                "uni",
                text,
                EventArgsContainer.Get(e),
                e.Message.Chat.Id,
                e.Message.Chat.Type,
                ParseMode.Html,
                e.Message.MessageId,
                true);
    }

    internal static void MessageInGroup2Async(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e,
        string text)
    {
        AutomaticAnswers.ForEach(x =>
        {
            if (telegramBotClient != null && e != null)
                x.TryTrigger(e, telegramBotClient, text);
        });
    }

    private static void MessageInGroup3Async(AutomaticAnswer answer, TelegramBotAbstract? telegramBotClient,
        MessageEventArgs? e, string text)
    {
        if (e == null || telegramBotClient == null)
            return;

        answer.TryTrigger(e, telegramBotClient, text);
    }
}