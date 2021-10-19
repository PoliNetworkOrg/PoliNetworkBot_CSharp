#region

using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class AskUser
    {
        public static readonly DictionaryUserAnswer UserAnswers = new DictionaryUserAnswer();

        internal static async Task<string> AskAsync(long idUser, Language question,
            TelegramBotAbstract sender, string lang, string username, bool sendMessageConfirmationChoice = false)
        {
            var botId = sender.GetId();

            UserAnswers.Reset(idUser, botId);

            await sender.SendTextMessageAsync(idUser, question, ChatType.Private, parseMode: default,
                replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.FORCED), lang: lang, username: username);

            var result = await WaitForAnswer(idUser, sendMessageConfirmationChoice, sender, lang, username);
            UserAnswers.Delete(idUser, botId);
            return result;
        }

        private static async Task<string> WaitForAnswer(long idUser, bool sendMessageConfirmationChoice,
            TelegramBotAbstract telegramBotAbstract, string lang, string username)
        {
            try
            {
                var botId = telegramBotAbstract.GetId();
                var tcs = UserAnswers.GetNewTCS(idUser, botId);
                UserAnswers.SetAnswerProcessed(idUser, botId, false);
                UserAnswers.AddWorkCompleted(idUser, botId, sendMessageConfirmationChoice, telegramBotAbstract, lang,
                    username);

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
            bool sendMessageConfirmationChoice = true, long? messageIdToReplyTo = 0)
        {
            var botId = sender.GetId();

            UserAnswers.Reset(idUser, botId);

            var replyMarkupObject = new ReplyMarkupObject(
                new ReplyMarkupOptions(
                    KeyboardMarkup.OptionsStringToKeyboard(options, lang)
                )
            );

            var m1 = await sender.SendTextMessageAsync(idUser, question, ChatType.Private,
                parseMode: default, replyMarkupObject: replyMarkupObject, lang: lang, username: username,
                replyToMessageId: messageIdToReplyTo);

            ;

            var result = await WaitForAnswer(idUser, sendMessageConfirmationChoice, sender, lang, username);
            ;
            UserAnswers.Delete(idUser, botId);
            return result;
        }

        internal static async Task<string> GetSedeAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var options = new List<List<Language>>
            {
                new List<Language> {new Language(new Dictionary<string, string> {{"en", "Milano Leonardo"}})},
                new List<Language> {new Language(new Dictionary<string, string> {{"en", "Milano Bovisa"}})},
                new List<Language> {new Language(new Dictionary<string, string> {{"en", "Como"}})}
            };
            var question = new Language(new Dictionary<string, string>
            {
                {"it", "In che sede?"},
                {"en", "In which territorial pole?"}
            });
            var reply = await AskBetweenRangeAsync(e.Message.From.Id,
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
            }

            return null;
        }

        internal static async Task<bool> AskYesNo(int id, Language question, bool defaultBool,
            TelegramBotAbstract sender, string lang, string username)
        {
            var l1 = new Language(new Dictionary<string, string>
            {
                {"it", "Si"},
                {"en", "Yes"}
            });
            var l2 = new Language(new Dictionary<string, string>
            {
                {"it", "No"},
                {"en", "No"}
            });

            var options = new List<List<Language>>
            {
                new List<Language>
                {
                    l1, l2
                }
            };

            var r = await AskBetweenRangeAsync(id, question, sender, lang, options, username);

            if (l1.Matches(r)) return true;

            if (l2.Matches(r)) return false;

            return defaultBool;
        }
    }
}