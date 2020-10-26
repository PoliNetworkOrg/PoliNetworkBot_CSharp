using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Threading.Tasks;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NotifyUtil
    {
        internal static async System.Threading.Tasks.Task NotifyOwners(Exception exception, TelegramBotAbstract sender)
        {
            Language text = new Language(dict: new System.Collections.Generic.Dictionary<string, string>() {
                    {"it", "Eccezione! " + exception.Message + "\n\n" + exception.ToString() },
                    {"en", "Exception! " + exception.Message + "\n\n" + exception.ToString()  }
                });

            await NotifyOwners2Async(text, sender);
        }

        private static async Task NotifyOwners2Async(Language text, TelegramBotAbstract sender)
        {
            const long group_exception = -438352042;
            await Utils.SendMessage.SendMessageInAGroup(sender, "en", text, group_exception,
                Telegram.Bot.Types.Enums.ChatType.Group, Telegram.Bot.Types.Enums.ParseMode.Default, null, true);
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
            await NotifyOwners(exception, sender);
        }

        internal static async Task NotifyOwners(string message, TelegramBotAbstract sender)
        {
            System.Collections.Generic.Dictionary<string, string> dict = new System.Collections.Generic.Dictionary<string, string>() {
                { "en", message}
            };
            Language text = new Language(dict: dict);
            await NotifyOwners2Async(text, sender);
        }
    }
}