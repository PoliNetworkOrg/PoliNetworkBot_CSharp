using PoliNetworkBot_CSharp.Code.Objects;
using System;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NotifyUtil
    {
        internal static async System.Threading.Tasks.Task NotifyOwners(Exception exception, TelegramBotAbstract sender)
        {
            foreach (Tuple<long, string> user in Data.GlobalVariables.Owners)
            {
                Language text = new Language(dict: new System.Collections.Generic.Dictionary<string, string>() {
                    {"it", "Eccezione! " + exception.Message + "\n\n" + exception.ToString() },
                    {"en", "Exception! " + exception.Message + "\n\n" + exception.ToString()  }
                });
                await Utils.SendMessage.SendMessageInPrivate(sender, usernameToSendTo: user.Item2,
                    userIdToSendTo: user.Item1, langCode: "en",
                    text: text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Default, messageIdToReplyTo: null);
            }
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
    }
}