#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Conversation;

public static class AutoReplyInGroups
{
    public static readonly Dictionary<SpecialGroup, long> ExcludedGroups = new()
    {
        { SpecialGroup.PIANO_DI_STUDI, -1001208900229 },
        { SpecialGroup.ASK_POLIMI, -1001251460298 },
        { SpecialGroup.DSU, -1001241129618 }
    };

    private static readonly Dictionary<SpecialGroup, List<SpecialGroup>> ExcludedGroupsMatch =
        new()
        {
            {
                SpecialGroup.ASK_POLIMI,
                new List<SpecialGroup> { SpecialGroup.ASK_POLIMI }
            },
            {
                SpecialGroup.DSU,
                new List<SpecialGroup> { SpecialGroup.DSU, SpecialGroup.ASK_POLIMI }
            },
            {
                SpecialGroup.PIANO_DI_STUDI,
                new List<SpecialGroup> { SpecialGroup.PIANO_DI_STUDI, SpecialGroup.ASK_POLIMI }
            }
        };

    private static bool CheckIfToSend(SpecialGroup s, long id)
    {
        var x = ExcludedGroupsMatch[s];
        return x.Select(i => ExcludedGroups[i]).All(j => id != j);
    }

    internal static async Task MessageInGroup2Async(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e,
        string text)
    {
        if (e?.Message != null && CheckIfToSend(SpecialGroup.PIANO_DI_STUDI, e.Message.Chat.Id))
            if (text.Contains("piano studi") || text.Contains("piano di studi") ||
                text.Contains("piano degli studi"))
            {
                var text2 = new Language(new Dictionary<string, string?>
                {
                    {
                        "it",
                        "Ciao 👋 sembra tu stia chiedendo domande in merito al piano di studi. " +
                        "PoliNetwork ti consiglia di scrivere nel gruppo dedicato, " +
                        "<a href='https://t.me/joinchat/aiAC6RgOjBRkYjhk'>clicca qui</a>!"
                    },
                    {
                        "en",
                        "Hi 👋 it seems you are asking questions about 'piano di studi'. " +
                        "PoliNetwork advice you to write in the dedicated group, " +
                        "<a href='https://t.me/joinchat/aiAC6RgOjBRkYjhk'>click here</a>!"
                    }
                });
                await SendMessage.SendMessageInAGroup(telegramBotClient,
                    e.Message.From?.LanguageCode,
                    text2,
                    e,
                    e.Message.Chat.Id,
                    e.Message.Chat.Type,
                    ParseMode.Html,
                    e.Message.MessageId,
                    true);
            }

        if (e?.Message != null && CheckIfToSend(SpecialGroup.ASK_POLIMI, e.Message.Chat.Id))
            if (text.ToLower().Contains("rappresentant") || text.ToLower().Contains("rappresentanza") ||
                text.ToLower().Contains("representative"))
            {
                var text2 = new Language(new Dictionary<string, string?>
                {
                    {
                        "it",
                        "Ciao 👋 sembra tu stia chiedendo domande in merito alla Rappresentanza. " +
                        "PoliNetwork ti consiglia di scrivere nel gruppo dedicato, " +
                        "<a href='https://t.me/askPolimi'>clicca qui</a>!"
                    },
                    {
                        "en",
                        "Hi 👋 it seems you are asking questions about Representatives. " +
                        "PoliNetwork advice you to write in the dedicated group, " +
                        "<a href='https://t.me/askPolimi'>click here</a>!"
                    }
                });
                await SendMessage.SendMessageInAGroup(telegramBotClient,
                    e.Message.From?.LanguageCode,
                    text2,
                    e,
                    e.Message.Chat.Id,
                    e.Message.Chat.Type,
                    ParseMode.Html,
                    e.Message.MessageId,
                    true);
            }

        if (e?.Message != null && CheckIfToSend(SpecialGroup.DSU, e.Message.Chat.Id))
            if (text.Contains("diritto studio universitario") || text.Contains("diritto allo studio") ||
                text.Contains("dsu"))
            {
                var text2 = new Language(new Dictionary<string, string?>
                {
                    {
                        "it",
                        "Ciao 👋 sembra tu stia chiedendo domande in merito al DSU. " +
                        "PoliNetwork ti consiglia di scrivere nel gruppo dedicato, " +
                        "<a href='https://t.me/joinchat/4kO9DtAiTVM0NTU0'>clicca qui</a>!"
                    },
                    {
                        "en",
                        "Hi 👋 it seems you are asking questions about 'DSU'. " +
                        "PoliNetwork advice you to write in the dedicated group, " +
                        "<a href='https://t.me/joinchat/4kO9DtAiTVM0NTU0'>click here</a>!"
                    }
                });
                await SendMessage.SendMessageInAGroup(telegramBotClient,
                    e.Message.From?.LanguageCode,
                    text2,
                    e,
                    e.Message.Chat.Id,
                    e.Message.Chat.Type,
                    ParseMode.Html,
                    e.Message.MessageId,
                    true);
            }

        if ((text.Contains("cento") || text.Contains("100")) &&
            text.Contains("maturità") &&
            (text.Contains("esonero") || text.Contains("sconto") || text.Contains("tasse")))
        {
            var text2 = new Language(new Dictionary<string, string?>
            {
                {
                    "it",
                    "No."
                },
                {
                    "en",
                    "No."
                }
            });
            if (e?.Message != null)
                await SendMessage.SendMessageInAGroup(telegramBotClient,
                    e.Message.From?.LanguageCode,
                    text2,
                    e,
                    e.Message.Chat.Id,
                    e.Message.Chat.Type,
                    ParseMode.Html,
                    e.Message.MessageId,
                    true);
        }
        
        if (text.Contains("quando") 
            && (text.Contains("presentare") || text.Contains("compilare") || text.Contains("modificare")) 
            && (text.Contains("piano studi") || text.Contains("piano di studi")))
        {
            var text2 = new Language(new Dictionary<string, string?>
            {
                {
                    "it",
                    "Ciao, forse la risposta è qui https://faq.polinetwork.org/?id=2&cat=1"
                }
            });
            if (e?.Message != null)
                await SendMessage.SendMessageInAGroup(telegramBotClient,
                    e.Message.From?.LanguageCode,
                    text2,
                    e,
                    e.Message.Chat.Id,
                    e.Message.Chat.Type,
                    ParseMode.Html,
                    e.Message.MessageId,
                    true);
        }


        if ((text.Contains("qualcun") || text.Contains("sa") || text.Contains("notiz"))
            && text.Contains("lezion") 
            && (text.Contains("online") || text.Contains("registrazion") || text.Contains("streaming")))
        {
            var text2 = new Language(new Dictionary<string, string?>
            {
                {
                    "it",
                    "Ciao, forse la risposta è qui https://faq.polinetwork.org/?id=1&lang=it&cat=3"
                }
            });
            if (e?.Message != null)
                await SendMessage.SendMessageInAGroup(telegramBotClient,
                    e.Message.From?.LanguageCode,
                    text2,
                    e,
                    e.Message.Chat.Id,
                    e.Message.Chat.Type,
                    ParseMode.Html,
                    e.Message.MessageId,
                    true);
        }
        

        if (e is { Message: { } } && e.Message.Chat.Title != null &&
            e.Message.Chat.Title.ToLower().Contains("matricole"))
            if ((text.Contains("link") || text.Contains("esiste")) && text.Contains("whatsapp") &&
                text.Contains("grupp"))
            {
                await CheckPinnedMessages(telegramBotClient, e);
            }

        if (text.Contains("esiste un gruppo"))
        {
            var text2 = new Language(
                new Dictionary<string, string?>
                {
                    {
                        "it",
                        "Ciao 👋 sembra tu stia chiedendo domande in merito ai gruppi. " +
                        "Ti consigliamo di visitare il nostro sito, " +
                        "<a href='https://polinetwork.github.io/'>clicca qui</a>!"
                    },
                    {
                        "en",
                        "Hi 👋 it seems you are asking questions about groups. " +
                        "We advice you to visit our website, " +
                        "<a href='https://polinetwork.github.io/'>click here</a>!"
                    }
                }
            );
            var message = e?.Message;
            if (message != null)
                await SendMessage.SendMessageInAGroup(telegramBotClient,
                    e?.Message?.From?.LanguageCode,
                    text2,
                    e,
                    message.Chat.Id,
                    message.Chat.Type,
                    ParseMode.Html,
                    message.MessageId,
                    true);
        }

        if (text.Contains("graduatorie") && (text.Contains("qualcuno") || text.Contains("punteggi")))
        {
            var text2 = new Language(
                new Dictionary<string, string?>
                {
                    {
                        "it",
                        "Ciao 👋 sembra tu stia chiedendo domande alle graduatorie degli anni passati. " +
                        "Ti consigliamo di visitare il nostro sito, " +
                        "<a href='https://polinetworkorg.github.io/Rankings/'>clicca qui</a>!"
                    },
                    {
                        "en",
                        "Hi 👋 it seems you are asking questions about rankings of previous years. " +
                        "We advice you to visit our website, " +
                        "<a href='https://polinetworkorg.github.io/Rankings/'>click here</a>!"
                    }
                }
            );
            var message = e?.Message;
            if (message != null)
                await SendMessage.SendMessageInAGroup(telegramBotClient,
                    e?.Message?.From?.LanguageCode,
                    text2,
                    e,
                    message.Chat.Id,
                    message.Chat.Type,
                    ParseMode.Html,
                    message.MessageId,
                    true);
        }

        if (DateTime.Now.Month is >= 1 and <= 6 or >= 11 and <= 12)
            if (text.Contains("whatsapp") && text.Contains("grupp"))
            {
                var text2 = new Language(
                    new Dictionary<string, string?>
                    {
                        {
                            "it",
                            "Ciao 👋 sembra tu stia chiedendo domande in merito ai gruppi whatsapp. " +
                            "Se non lo hai ancora fatto, leggi la guida in merito, " +
                            "<a href='https://docs.polinetwork.org/#/it/about/groups/whatsapp'>clicca qui</a>!"
                        },
                        {
                            "en",
                            "Hi 👋 it seems you are asking questions about whatsapp groups. " +
                            "If you have not already done it, we advice you to read the relative guide, " +
                            "<a href='https://docs.polinetwork.org/#/en/about/groups/whatsapp'>click here</a>!"
                        }
                    }
                );


                var message = e?.Message;
                if (message != null)
                    await SendMessage.SendMessageInAGroup(telegramBotClient,
                        message.From?.LanguageCode,
                        text2,
                        e,
                        message.Chat.Id,
                        message.Chat.Type,
                        ParseMode.Html,
                        message.MessageId,
                        true);
            }
    }

    private static async Task CheckPinnedMessages(TelegramBotAbstract? telegramBotClient, MessageEventArgs e)
    {
        var text2 = new Language(new Dictionary<string, string?>
        {
            {
                "it",
                "Controlla i messaggi fissati"
            },
            {
                "en",
                "Check the pinned messages"
            }
        });
        if (e.Message != null)
            await SendMessage.SendMessageInAGroup(telegramBotClient,
                e.Message.From?.LanguageCode,
                text2,
                e,
                e.Message.Chat.Id,
                e.Message.Chat.Type,
                ParseMode.Html,
                e.Message.MessageId,
                true);
    }
}