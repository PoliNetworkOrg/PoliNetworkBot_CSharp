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
                    text: text, html: Telegram.Bot.Types.Enums.ParseMode.Default);
            }
        }
    }
}