using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NotifyUtil
    {
        const long group_exception = -438352042;
        const string default_lang = "en";

        internal static async System.Threading.Tasks.Task NotifyOwners(ExceptionNumbered exception, TelegramBotAbstract sender, int v = 0, string extrainfo = null, string langCode = default_lang)
        {
            if (sender == null)
                return;

            string message = exception.Message + "\n\n" + exception.GetException().ToString();
            if (!string.IsNullOrEmpty(extrainfo))
            {
                message += "\n\n" + extrainfo;
            }

            Language text = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Eccezione! " + message },
                    {"en", "Exception! " + message }
                });

            var r1 = await NotifyOwners2Async(text, sender, v, langCode);
            if (r1 == null)
                return;

            if (r1.IsSuccess())
            {
                int v2 = exception.GetNumberOfTimes();
                if (v <= 1)
                {
                    return;
                }

                string message2 = v2.ToString();

                Language text2 = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Numero di volte: " + message2 },
                    {"en", "Number of times: " + message2 }
                });

                long? replyToMessageId = r1.GetMessageID();

                await NotifyOwners3(text2, sender, replyToMessageId, v, langCode);
            }
        }

        private static async Task<MessageSend> NotifyOwners3(Language text2, TelegramBotAbstract sender, long? replyToMessageId, int v, string langCode)
        {
            return await SendMessage.SendMessageInAGroup(sender, langCode, text2, group_exception,
                    Telegram.Bot.Types.Enums.ChatType.Group, Telegram.Bot.Types.Enums.ParseMode.Default, replyToMessageId: replyToMessageId, true, v);
        }

        internal static async Task NotifyOwners(Exception e, TelegramBotAbstract telegramBotAbstract, int v = 0)
        {
            await NotifyOwners(new ExceptionNumbered(e), telegramBotAbstract, v);
        }

        private static async Task<MessageSend> NotifyOwners2Async(Language text, TelegramBotAbstract sender, int v, string langCode)
        {
            return await NotifyOwners3(text, sender, null, v, langCode);
        }

        internal static async System.Threading.Tasks.Task NotifyIfFalseAsync(Tuple<bool?, string, long> r1, string extraInfo, TelegramBotAbstract sender)
        {
            if (r1 == null)
                return;

            if (r1.Item1 == null)
                return;

            if (r1.Item1.Value)
                return;

            string error = "Error (notifyIfFalse): ";
            error += "\n";
            error += "String: " + r1?.Item2 + "\n";
            error += "Long: " + r1.Item3.ToString() + "\n";
            error += "Extra: " + extraInfo;
            error += "\n";

            ExceptionNumbered exception = new ExceptionNumbered(error);
            await NotifyOwners(exception, sender, 0);
        }

        internal static async Task NotifyOwners(Exception item2, string message, TelegramBotAbstract sender, string langCode)
        {
            System.Collections.Generic.Dictionary<string, string> dict = new System.Collections.Generic.Dictionary<string, string>() {
                { "en", message}
            };
            Language text = new Language(dict: dict);
            await NotifyOwners2Async(text, sender, 0, langCode);
        }

        internal static async Task NotifyOwnersAsync(List<ExceptionNumbered> exceptions, TelegramBotAbstract sender, string v, string langCode)
        {
            Language text = new Language(dict: new Dictionary<string, string>() {
                { "en", v }
            }) ;
            await NotifyOwners2Async(text, sender, 0, langCode);

            foreach (var e1 in exceptions)
            {
                await NotifyOwners(e1, sender, 0);
            }

            Language text2 = new Language(dict: new Dictionary<string, string>() {
                { "en", "---End---"}
            });
            await NotifyOwners2Async(text2, sender, 0, langCode);
        }
    }
}