using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class SendMessage
    {
        internal static void SendMessageInPrivateOrAGroup(TelegramBotAbstract telegramBotClient, MessageEventArgs e, string text)
        {
            try
            {
                telegramBotClient.SendTextMessageAsync(e.Message.From.Id, text, Telegram.Bot.Types.Enums.ChatType.Private, Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            catch
            {
                string message_to = GetMessageTo(e);
                string message_for = "Messaggio per";
                string language = e.Message.From.LanguageCode.ToLower();
                switch (language)
                {
                    case "en":
                        message_for = "Message for";
                        break;
                }
                string text2 = "[" + message_for + " " + message_to + "]\n\n" + text;
                telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id, text2, e.Message.Chat.Type, Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }

        private static string GetMessageTo(MessageEventArgs e)
        {
            string name = e.Message.From.FirstName + " " + e.Message.From.LastName;
            name = name.Trim();
            return "<a href=\"tg://user?id=" + e.Message.From.Id + "\">" + name + "</a>";
        }

        internal static bool SendMessageInPrivate(TelegramBotAbstract telegramBotClient, MessageEventArgs e, string text, Telegram.Bot.Types.Enums.ParseMode html = Telegram.Bot.Types.Enums.ParseMode.Default)
        {
            try
            {
                telegramBotClient.SendTextMessageAsync(e.Message.From.Id, text, Telegram.Bot.Types.Enums.ChatType.Private, html);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static async System.Threading.Tasks.Task<bool> SendFileAsync(TelegramFile File, long chat_id, 
            string text, TextAsCaption text_as_caption, TelegramBotAbstract TelegramBot_Abstract)
        {
            return await TelegramBot_Abstract.SendFileAsync(File, chat_id, text, text_as_caption);
        }
    }
}