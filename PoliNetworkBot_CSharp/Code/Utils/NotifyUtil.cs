using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NotifyUtil
    {
        internal static async System.Threading.Tasks.Task NotifyOwners(Exception exception, TelegramBotAbstract sender, int v = 0, string extrainfo = null)
        {
            if (sender == null)
                return;

            string message = exception.Message + "\n\n" + exception.ToString();
            if (!string.IsNullOrEmpty(extrainfo))
            {
                message += "\n\n" + extrainfo;
            }

            Language text = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Eccezione! " + message },
                    {"en", "Exception! " + message }
                });

            await NotifyOwners2Async(text, sender, v);
        }

        private static async Task NotifyOwners2Async(Language text, TelegramBotAbstract sender, int v)
        {
            const long group_exception = -438352042;
            await Utils.SendMessage.SendMessageInAGroup(sender, "en", text, group_exception,
                Telegram.Bot.Types.Enums.ChatType.Group, Telegram.Bot.Types.Enums.ParseMode.Default, null, true, v);
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

            Exception exception = new Exception(error);
            await NotifyOwners(exception, sender, 0);
        }

        internal static async Task NotifyOwners(Exception item2, string message, TelegramBotAbstract sender)
        {
            System.Collections.Generic.Dictionary<string, string> dict = new System.Collections.Generic.Dictionary<string, string>() {
                { "en", message}
            };
            Language text = new Language(dict: dict);
            await NotifyOwners2Async(text, sender, 0);
        }

        internal static async Task NotifyOwnersAsync(List<Exception> exceptions, TelegramBotAbstract sender, string v)
        {
            Language text = new Language(dict: new Dictionary<string, string>() {
                { "en", v }
            }) ;
            await NotifyOwners2Async(text, sender, 0);

            foreach (var e1 in exceptions)
            {
                await NotifyOwners(e1, sender, 0);
            }

            Language text2 = new Language(dict: new Dictionary<string, string>() {
                { "en", "---End---"}
            });
            await NotifyOwners2Async(text2, sender, 0);
        }
    }
}