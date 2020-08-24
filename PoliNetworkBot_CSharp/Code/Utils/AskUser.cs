using PoliNetworkBot_CSharp.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class AskUser
    {
        public static Dictionary<int, AnswerTelegram> userAnswers = new Dictionary<int, AnswerTelegram>();

        internal static async Task<string> AskAsync(int id, Dictionary<string, string> dictionary, TelegramBotAbstract sender, string lang)
        {
            string to_send = dictionary[lang];
            userAnswers[id] = new AnswerTelegram();
            sender.SendTextMessageAsync(id, to_send, Telegram.Bot.Types.Enums.ChatType.Private, parseMode: default, force_reply: true);
            return await WaitForAnswer(id);
        }

        private async static Task<string> WaitForAnswer(int id)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            userAnswers[id].WorkCompleted += result => tcs.SetResult(result.ToString());
            return await tcs.Task;
        }

        internal static async Task<string> AskBetweenRangeAsync(int id, Dictionary<string, string> language,
            TelegramBotAbstract sender, string lang, List<List<string>> options)
        {
            string to_send = language[lang];
            userAnswers[id] = new AnswerTelegram();
            sender.SendTextMessageAsync(id, to_send, Telegram.Bot.Types.Enums.ChatType.Private, parseMode: default,
                force_reply: true, reply_markup_keyboard: OptionsStringToKeyboard(options));
            return await WaitForAnswer(id);
        }

        private static List<List<KeyboardButton>> OptionsStringToKeyboard(List<List<string>> options)
        {
            return options.Select(o => o.Select(o2 => new KeyboardButton(o2)).ToList()).ToList();
        }
    }
}