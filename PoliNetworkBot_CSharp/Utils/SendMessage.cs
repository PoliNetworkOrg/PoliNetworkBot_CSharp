using System;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class SendMessage
    {
        internal static void SendMessageInPrivateOrAGroup(TelegramBotClient telegramBotClient, MessageEventArgs e, string text)
        {
            try
            {

                telegramBotClient.SendTextMessageAsync(e.Message.From.Id, text, Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            catch
            {
                string message_to = GetMessageTo(e);
                string text2 = "[Messaggio per "+message_to+"]\n\n" + text;
                telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id, text2);
            }
        }

        private static string GetMessageTo(MessageEventArgs e)
        {
            string name = e.Message.From.FirstName + " " + e.Message.From.LastName;
            name = name.Trim();
            return "<a href=\"tg://user?id="+e.Message.From.Id+"\">"+name+"</a>";
        }
    }
}