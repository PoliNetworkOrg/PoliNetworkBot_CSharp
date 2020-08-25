#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class AskUser
    {
        public static readonly Dictionary<int, AnswerTelegram> UserAnswers = new Dictionary<int, AnswerTelegram>();

        internal static async Task<string> AskAsync(int id, Dictionary<string, string> dictionary,
            TelegramBotAbstract sender, string lang)
        {
            var toSend = dictionary[lang];
            UserAnswers[id] = new AnswerTelegram();
            sender.SendTextMessageAsync(id, toSend, ChatType.Private, default, true);
            return await WaitForAnswer(id);
        }

        private static async Task<string> WaitForAnswer(int id)
        {
            var tcs = new TaskCompletionSource<string>();
            UserAnswers[id].WorkCompleted += result => tcs.SetResult(result.ToString());
            return await tcs.Task;
        }

        internal static async Task<string> AskBetweenRangeAsync(int id, Language question,
            TelegramBotAbstract sender, string lang, IEnumerable<List<Language>> options)
        {
            var toSend = question.Select(lang);
            UserAnswers[id] = new AnswerTelegram();
            sender.SendTextMessageAsync(id, toSend, ChatType.Private, default,
                true, OptionsStringToKeyboard(options, lang));
            return await WaitForAnswer(id);
        }

        private static List<List<KeyboardButton>> OptionsStringToKeyboard(IEnumerable<List<Language>> options,
            string lang)
        {
            return options.Select(o => o.Select(
                o2 =>
                {
                    var o3 = o2.Select(lang);
                    return new KeyboardButton(o3);
                }
            ).ToList()).ToList();
        }
    }
}