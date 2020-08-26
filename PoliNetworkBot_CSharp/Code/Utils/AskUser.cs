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

        internal static async Task<string> AskAsync(long idUser, Dictionary<string, string> dictionary,
            TelegramBotAbstract sender, string lang, bool sendMessageConfirmationChoice = false)
        {
            var toSend = dictionary[lang];
            UserAnswers[idUser] = new AnswerTelegram();
            await sender.SendTextMessageAsync(idUser, toSend, ChatType.Private, default,
                new ReplyMarkupObject(ReplyMarkupEnum.FORCED));
            return await WaitForAnswer(idUser, sendMessageConfirmationChoice, sender);
        }

        private static async Task<string> WaitForAnswer(long idUser, bool sendMessageConfirmationChoice,
            TelegramBotAbstract telegramBotAbstract)
        {
            var tcs = new TaskCompletionSource<string>();
            UserAnswers[idUser].WorkCompleted += async result =>
            {
                if (sendMessageConfirmationChoice)
                {
                    var replyMarkup = new ReplyMarkupObject(ReplyMarkupEnum.REMOVE);
                    await telegramBotAbstract.SendTextMessageAsync(idUser,
                        "You choose [" + result + "]",
                        ChatType.Private, default, replyMarkup);
                }

                tcs.SetResult(result.ToString());
            };
            return await tcs.Task;
        }

        internal static async Task<string> AskBetweenRangeAsync(int id, Language question,
            TelegramBotAbstract sender, string lang, IEnumerable<List<Language>> options,
            bool sendMessageConfirmationChoice = true)
        {
            var toSend = question.Select(lang);
            UserAnswers[id] = new AnswerTelegram();
            var replyMarkupObject = new ReplyMarkupObject(
                new ReplyMarkupOptions( 
                    KeyboardMarkup.OptionsStringToKeyboard(options, lang)
                    )
                );

            await sender.SendTextMessageAsync(id, toSend, ChatType.Private, default, replyMarkupObject);
            return await WaitForAnswer(id, sendMessageConfirmationChoice, sender);
        }
    }
}