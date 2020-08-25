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

        internal static async Task<string> AskBetweenRangeAsync(int id, Dictionary<string, string> language,
            TelegramBotAbstract sender, string lang, IEnumerable<List<string>> options)
        {
            var toSend = language[lang];
            UserAnswers[id] = new AnswerTelegram();
            sender.SendTextMessageAsync(id, toSend, ChatType.Private, default,
                true, OptionsStringToKeyboard(options));
            return await WaitForAnswer(id);
        }

        private static List<List<KeyboardButton>> OptionsStringToKeyboard(IEnumerable<List<string>> options)
        {
            return options.Select(o => o.Select(o2 => new KeyboardButton(o2)).ToList()).ToList();
        }
    }
}