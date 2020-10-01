#region

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
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
                                "en",
                                 "Hi 👋 it seems you are asking questions about 'piano di studi'. " +
                                "PoliNetwork ti consiglia di scrivere nel gruppo dedicato, " +
                                 "<a href='https://t.me/joinchat/FNGD_0gOWoXdxdMhwUeMdw'>clicca qui</a>!"
                            },
                            {
                                "it",
                                "Ciao 👋 sembra tu stia chiedendo domande in merito al piano di studi. " +
                                "PoliNetwork advice you to write in the dedicated group, " +
                                 "<a href='https://t.me/joinchat/FNGD_0gOWoXdxdMhwUeMdw'>click here</a>!"
                            }
                        });
                        await SendMessage.SendMessageInAGroup(telegramBotClient: telegramBotClient,
                            userId: e.Message.From.Id,
                            lang: e.Message.From.LanguageCode,
                            username: e.Message.From.Username,
                            text: text2, firstName: e.Message.From.FirstName,
                            lastName: e.Message.From.LastName,
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
            if (AskUser.UserAnswers.ContainsKey(e.Message.From.Id))
                if (AskUser.UserAnswers[e.Message.From.Id] != null)
                    if (AskUser.UserAnswers[e.Message.From.Id].GetState() == AnswerTelegram.State.WAITING_FOR_ANSWER)
                    {
                        AskUser.UserAnswers[e.Message.From.Id].RecordAnswer(e.Message.Text);
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
e.Message.From.Username, text2);
        }
    }
}