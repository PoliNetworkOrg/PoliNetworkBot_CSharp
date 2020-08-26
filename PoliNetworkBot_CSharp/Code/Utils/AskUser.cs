#region

using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class AskUser
    {
        public static readonly Dictionary<long, AnswerTelegram> UserAnswers = new Dictionary<long, AnswerTelegram>();

        internal static async Task<string> AskAsync(long idUser, Language question,
            TelegramBotAbstract sender, string lang, string username, bool sendMessageConfirmationChoice = false)
        {
            UserAnswers[idUser] = new AnswerTelegram();
            await sender.SendTextMessageAsync(idUser, question, ChatType.Private, parseMode: default,
                replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.FORCED), lang: lang, username: username);
            return await WaitForAnswer(idUser, sendMessageConfirmationChoice, sender, lang, username);
        }

        private static async Task<string> WaitForAnswer(long idUser, bool sendMessageConfirmationChoice,
            TelegramBotAbstract telegramBotAbstract, string lang, string username)
        {
            var tcs = new TaskCompletionSource<string>();
            UserAnswers[idUser].WorkCompleted += async result =>
            {
                if (sendMessageConfirmationChoice)
                {
                    var replyMarkup = new ReplyMarkupObject(ReplyMarkupEnum.REMOVE);
                    var languagereply = new Language(dict: new Dictionary<string, string>()
                    {
                        {"en", "You choose [" + result + "]"},
                        {"it", "Hai scelto [" + result + "]"}
                    });
                    await telegramBotAbstract.SendTextMessageAsync(idUser,
                        languagereply,
                        ChatType.Private, lang: lang, parseMode:default, replyMarkup, username);
                }

                tcs.SetResult(result.ToString());
            };
            return await tcs.Task;
        }

        internal static async Task<string> AskBetweenRangeAsync(int id, Language question,
            TelegramBotAbstract sender, string lang, IEnumerable<List<Language>> options,
            string username,
            bool sendMessageConfirmationChoice = true)
        {
            UserAnswers[id] = new AnswerTelegram();
            var replyMarkupObject = new ReplyMarkupObject(
                new ReplyMarkupOptions(
                    KeyboardMarkup.OptionsStringToKeyboard(options, lang)
                )
            );

            await sender.SendTextMessageAsync(chatid: id, text:  question, chatType: ChatType.Private, 
                parseMode: default, replyMarkupObject: replyMarkupObject, lang: lang, username: username);
            return await WaitForAnswer(id, sendMessageConfirmationChoice, sender, lang, username);
        }
    }
}