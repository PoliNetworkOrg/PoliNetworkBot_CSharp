#region

using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class SendMessage
    {
        internal static void SendMessageInPrivateOrAGroup(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
            string text)
        {
            try
            {
                telegramBotClient.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, ParseMode.Html);
            }
            catch
            {
                var message_to = GetMessageTo(e);
                var message_for = "Messaggio per";
                var language = e.Message.From.LanguageCode.ToLower();
                switch (language)
                {
                    case "en":
                        message_for = "Message for";
                        break;
                }

                var text2 = "[" + message_for + " " + message_to + "]\n\n" + text;
                telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id, text2, e.Message.Chat.Type, ParseMode.Html);
            }
        }

        private static string GetMessageTo(MessageEventArgs e)
        {
            var name = e.Message.From.FirstName + " " + e.Message.From.LastName;
            name = name.Trim();
            return "<a href=\"tg://user?id=" + e.Message.From.Id + "\">" + name + "</a>";
        }

        internal static bool SendMessageInPrivate(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
            string text, ParseMode html = ParseMode.Default)
        {
            try
            {
                telegramBotClient.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, html);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static async Task<bool> SendFileAsync(TelegramFile File, long chat_id,
            string text, TextAsCaption text_as_caption, TelegramBotAbstract TelegramBot_Abstract)
        {
            return await TelegramBot_Abstract.SendFileAsync(File, chat_id, text, text_as_caption);
        }
    }
}