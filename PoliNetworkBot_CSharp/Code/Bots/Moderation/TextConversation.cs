#region

using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class TextConversation
    {
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

        private async static Task<object> MessageInGroup(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            if (e == null)
                return null;

            if (e.Message == null)
                return null;

            if (string.IsNullOrEmpty(e.Message.Text))
                return null;

            if (e.Message.Chat == null)
                return null;

            string text = e.Message.Text.ToLower();
            if (e.Message.Chat.Title.ToLower().Contains("polimi"))
            {
                if (e.Message.Chat.Id != -1001208900229)
                {
                    if (text.Contains("piano studi") || text.Contains("piano di studi") || text.Contains("piano degli studi"))
                    {
                        var text2 = new Language(new Dictionary<string, string>
                        {
                            {
                                "it",
                                "Ciao 👋 sembra tu stia chiedendo domande in merito al piano di studi. " +
                                "PoliNetwork ti consiglia di scrivere nel gruppo dedicato, " +
                                 "<a href='https://t.me/joinchat/LclXl0gOWoUCf67O6nwnnQ'>clicca qui</a>!"
                            },
                            {
                                "en",
                                "Hi 👋 it seems you are asking questions about 'piano di studi'. " +
                                "PoliNetwork advice you to write in the dedicated group, " +
                                 "<a href='https://t.me/joinchat/LclXl0gOWoUCf67O6nwnnQ'>click here</a>!"
                            }
                        });
                        await SendMessage.SendMessageInAGroup(telegramBotClient: telegramBotClient,
                            lang: e.Message.From.LanguageCode,
                            text: text2,
                            chatId: e.Message.Chat.Id,
                            chatType: e.Message.Chat.Type,
                            parseMode: ParseMode.Html,
                            replyToMessageId: e.Message.MessageId,
                            disablePreviewLink: true);
                    }
                }

                if (e.Message.Chat.Id != -1001241129618)
                {
                    if (text.Contains("diritto studio universitario") || text.Contains("diritto allo studio") || text.Contains("dsu"))
                    {
                        var text2 = new Language(new Dictionary<string, string>
                        {
                            {
                                "it",
                                "Ciao 👋 sembra tu stia chiedendo domande in merito al DSU. " +
                                "PoliNetwork ti consiglia di scrivere nel gruppo dedicato, " +
                                 "<a href='https://t.me/joinchat/LclXl0n6IpKEeo39TbOkRw'>clicca qui</a>!"
                            },
                            {
                                "en",
                                "Hi 👋 it seems you are asking questions about 'DSU'. " +
                                "PoliNetwork advice you to write in the dedicated group, " +
                                 "<a href='https://t.me/joinchat/LclXl0n6IpKEeo39TbOkRw'>click here</a>!"
                            }
                        });
                        await SendMessage.SendMessageInAGroup(telegramBotClient: telegramBotClient,
                            lang: e.Message.From.LanguageCode,
                            text: text2,
                            chatId: e.Message.Chat.Id,
                            chatType: e.Message.Chat.Type,
                            parseMode: ParseMode.Html,
                            replyToMessageId: e.Message.MessageId,
                            disablePreviewLink: true);
                    }
                }
            }

            return null;
        }

        private static async Task PrivateMessage(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            long botId = telegramBotClient.GetId();

            if (AskUser.UserAnswers.ContainsUser(e.Message.From.Id, botId))
                if (AskUser.UserAnswers.GetState(e.Message.From.Id, botId) == AnswerTelegram.State.WAITING_FOR_ANSWER)
                {
                    AskUser.UserAnswers.RecordAnswer(e.Message.From.Id, botId, e.Message.Text);
                    return;
                }

            if (string.IsNullOrEmpty(e.Message.Text))
            {
                return;
            }

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
                parseMode: ParseMode.Default, null);
        }
    }
}