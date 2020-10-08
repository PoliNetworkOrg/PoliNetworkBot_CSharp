#region

using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Args;
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
            UserAnswers[idUser] = null;
            UserAnswers[idUser] = new AnswerTelegram();
            UserAnswers[idUser].Reset();

            await sender.SendTextMessageAsync(idUser, question, ChatType.Private, parseMode: default,
                replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.FORCED), lang: lang, username: username);

            var result = await WaitForAnswer(idUser, sendMessageConfirmationChoice, sender, lang, username);
            UserAnswers[idUser] = null;
            return result;
        }

        private static async Task<string> WaitForAnswer(long idUser, bool sendMessageConfirmationChoice,
            TelegramBotAbstract telegramBotAbstract, string lang, string username)
        {
            try
            {
                var tcs = new TaskCompletionSource<string>();
                UserAnswers[idUser].SetAnswerProcessed(false);

                UserAnswers[idUser].WorkCompleted += async result =>
                {
                    bool crashed = true;
                    try
                    {
                        if (UserAnswers[idUser].GetState() == AnswerTelegram.State.ANSWERED && UserAnswers[idUser].GetAlreadyProcessedAnswer() == false)
                        {
                            if (sendMessageConfirmationChoice)
                            {
                                var replyMarkup = new ReplyMarkupObject(ReplyMarkupEnum.REMOVE);
                                var languageReply = new Language(new Dictionary<string, string>
                            {
                                {"en", "You choose [" + result + "]"},
                                {"it", "Hai scelto [" + result + "]"}
                            });
                                await telegramBotAbstract.SendTextMessageAsync(idUser,
                                    languageReply,
                                    ChatType.Private, lang, default, replyMarkup, username);
                            }

                            UserAnswers[idUser].SetAnswerProcessed(true);

                            var resultstring = result.ToString();
                            crashed = false;
                            var done = tcs.TrySetResult(resultstring);
                        }
                    }
                    catch
                    {
                        ;
                    }

                    if (crashed)
                    {
                        tcs.TrySetResult("");
                    }
                };

                return await tcs.Task;
            }
            catch
            {
                ;
            }

            return null;
        }

        internal static async Task<string> AskBetweenRangeAsync(int idUser, Language question,
            TelegramBotAbstract sender, string lang, IEnumerable<List<Language>> options,
            string username,
            bool sendMessageConfirmationChoice = true)
        {
            UserAnswers[idUser] = null;
            UserAnswers[idUser] = new AnswerTelegram();
            UserAnswers[idUser].Reset();

            var replyMarkupObject = new ReplyMarkupObject(
                new ReplyMarkupOptions(
                    KeyboardMarkup.OptionsStringToKeyboard(options, lang)
                )
            );

            await sender.SendTextMessageAsync(idUser, question, ChatType.Private,
                parseMode: default, replyMarkupObject: replyMarkupObject, lang: lang, username: username);
            var result = await WaitForAnswer(idUser, sendMessageConfirmationChoice, sender, lang, username);
            UserAnswers[idUser] = null;
            return result;
        }

        internal static async Task<string> GetSedeAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            List<List<Language>> options = new List<List<Language>>
            {
                new List<Language>() { new Language(dict: new Dictionary<string, string>() { { "en", "Milano Leonardo" } }) },
                new List<Language>() { new Language(dict: new Dictionary<string, string>() { { "en", "Milano Bovisa" } }) },
                new List<Language>() { new Language(dict: new Dictionary<string, string>() { { "en", "Como" } }) }
            };
            Language question = new Language(dict: new Dictionary<string, string>() {
                {"it", "In che sede?" },
                {"en", "In which territorial pole?" }
            });
            var reply = await Utils.AskUser.AskBetweenRangeAsync(idUser: e.Message.From.Id,
                sender: sender,
                lang: e.Message.From.LanguageCode,
                options: options,
                username: e.Message.From.Username,
                sendMessageConfirmationChoice: true,
                question: question);

            if (string.IsNullOrEmpty(reply))
                return null;

            switch (reply)
            {
                case "Milano Leonardo":
                    return "MIA";

                case "Milano Bovisa":
                    return "MIB";

                case "Como":
                    return "COE";

                default:
                    break;
            }

            return null;
        }
    }
}