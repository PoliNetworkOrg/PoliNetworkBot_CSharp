#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class TextConversation
    {
        public static Dictionary<SpecialGroup, long> excludedGroups = new()
        {
            { SpecialGroup.PIANO_DI_STUDI, -1001208900229 },
            { SpecialGroup.ASK_POLIMI, -1001251460298 },
            { SpecialGroup.DSU, -1001241129618 }
        };

        public static Dictionary<SpecialGroup, List<SpecialGroup>> excludedGroupsMatch =
            new()
            {
                { SpecialGroup.ASK_POLIMI, new List<SpecialGroup> { SpecialGroup.ASK_POLIMI } },
                { SpecialGroup.DSU, new List<SpecialGroup> { SpecialGroup.DSU, SpecialGroup.ASK_POLIMI } },
                {
                    SpecialGroup.PIANO_DI_STUDI,
                    new List<SpecialGroup> { SpecialGroup.PIANO_DI_STUDI, SpecialGroup.ASK_POLIMI }
                }
            };

        internal static async Task DetectMessage(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            switch (e.Message.Chat.Type)
            {
                case ChatType.Private:
                    {
                        await PrivateMessage(telegramBotClient, e);
                        break;
                    }
                case ChatType.Channel:
                    break;

                case ChatType.Group:
                case ChatType.Supergroup:
                    {
                        await MessageInGroup(telegramBotClient, e);
                        break;
                    }
            }
        }

        private static async Task MessageInGroup(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            if (e == null)
                return;

            if (e.Message == null)
                return;

            if (string.IsNullOrEmpty(e.Message.Text))
                return;

            if (e.Message.Chat == null)
                return;

            var text = e.Message.Text.ToLower();
            if (e.Message.Chat.Title.ToLower().Contains("polimi"))
            {
                await MessageInGroup2Async(telegramBotClient, e, text);
            }
        }

        private static async Task MessageInGroup2Async(TelegramBotAbstract telegramBotClient, MessageEventArgs e, string text)
        {
            if (CheckIfToSend(SpecialGroup.PIANO_DI_STUDI, e.Message.Chat.Id))
                if (text.Contains("piano studi") || text.Contains("piano di studi") ||
                    text.Contains("piano degli studi"))
                {
                    var text2 = new Language(new Dictionary<string, string>
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
                        e.Message.From.LanguageCode,
                        text2,
                        e.Message.Chat.Id,
                        e.Message.Chat.Type,
                        ParseMode.Html,
                        e.Message.MessageId,
                        true);
                }

            if (CheckIfToSend(SpecialGroup.ASK_POLIMI, e.Message.Chat.Id))
                if (text.ToLower().Contains("rappresentant") || text.ToLower().Contains("rappresentanza") ||
                    text.ToLower().Contains("representative"))
                {
                    var text2 = new Language(new Dictionary<string, string>
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
                        e.Message.From.LanguageCode,
                        text2,
                        e.Message.Chat.Id,
                        e.Message.Chat.Type,
                        ParseMode.Html,
                        e.Message.MessageId,
                        true);
                }

            if (CheckIfToSend(SpecialGroup.DSU, e.Message.Chat.Id))
                if (text.Contains("diritto studio universitario") || text.Contains("diritto allo studio") ||
                    text.Contains("dsu"))
                {
                    var text2 = new Language(new Dictionary<string, string>
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
                        e.Message.From.LanguageCode,
                        text2,
                        e.Message.Chat.Id,
                        e.Message.Chat.Type,
                        ParseMode.Html,
                        e.Message.MessageId,
                        true);
                }

            if (text.Contains("esiste un gruppo"))
            {
                var text2 = new Language(
                    new Dictionary<string, string>
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
                await SendMessage.SendMessageInAGroup(telegramBotClient,
                    e.Message.From.LanguageCode,
                    text2,
                    e.Message.Chat.Id,
                    e.Message.Chat.Type,
                    ParseMode.Html,
                    e.Message.MessageId,
                    true);
            }

            if ((DateTime.Now.Month >= 1 && DateTime.Now.Month <= 6) || (DateTime.Now.Month >= 11 && DateTime.Now.Month <= 12))
            {
                if (text.Contains("whatsapp") && (text.Contains("grupp")))
                {
                    var text2 = new Language(
                        new Dictionary<string, string>
                        {
                            {
                                "it",
                                "Ciao 👋 sembra tu stia chiedendo domande in merito ai gruppi whatsapp. " +
                                "Se non lo hai ancora fatto, leggi la guida in merito, " +
                                "<a href='https://docs.polinetwork.org/#/about/groups/whatsapp'>clicca qui</a>!"
                            },
                            {
                                "en",
                                "Hi 👋 it seems you are asking questions about whatsapp groups. " +
                                "If you have not already done it, we advice you to read the relative guide, " +
                                "<a href='https://docs.polinetwork.org/#/about/groups/whatsapp'>click here</a>!"
                            }
                        }
                    );
                    await SendMessage.SendMessageInAGroup(telegramBotClient,
                        e.Message.From.LanguageCode,
                        text2,
                        e.Message.Chat.Id,
                        e.Message.Chat.Type,
                        ParseMode.Html,
                        e.Message.MessageId,
                        true);
                }
            }
        }

        private static bool CheckIfToSend(SpecialGroup s, long id)
        {
            var x = excludedGroupsMatch[s];
            foreach (var i in x)
            {
                var j = excludedGroups[i];
                if (id == j)
                    return false;
            }

            return true;
        }

        private static async Task PrivateMessage(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            var botId = telegramBotClient.GetId();

            if (AskUser.UserAnswers.ContainsUser(e.Message.From.Id, botId))
                if (AskUser.UserAnswers.GetState(e.Message.From.Id, botId) == AnswerTelegram.State.WAITING_FOR_ANSWER)
                {
                    AskUser.UserAnswers.RecordAnswer(e.Message.From.Id, botId, e.Message.Text);
                    return;
                }

            if (string.IsNullOrEmpty(e.Message.Text)) return;

            var text2 = new Language(new Dictionary<string, string>
            {
                {
                    "en",
                    "Hi, at the moment is not possible to have conversation with the bot.\n" +
                    "We advice you to write /help to see what this bot can do"
                },
                {
                    "it",
                    "Ciao, al momento non è possibile fare conversazione col bot.\n" +
                    "Ti consigliamo di premere /help per vedere le funzioni disponibili"
                }
            });
            await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2,
                ParseMode.Html, null);
        }
    }
}