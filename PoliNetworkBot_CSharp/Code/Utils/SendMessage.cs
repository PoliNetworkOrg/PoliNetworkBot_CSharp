#region

using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class SendMessage
    {
        internal static async Task<bool> SendMessageInPrivateOrAGroup(TelegramBotAbstract telegramBotClient,
            MessageEventArgs e,
            string text)
        {
            try
            {
                var r = await telegramBotClient.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                    ParseMode.Html);
                if (r) return true;
            }
            catch
            {
                // ignored
            }

            var messageTo = GetMessageTo(e);
            var messageFor = "Messaggio per";
            var language = e.Message.From.LanguageCode.ToLower();
            messageFor = language switch
            {
                "en" => "Message for",
                _ => messageFor
            };

            var text2 = "[" + messageFor + " " + messageTo + "]\n\n" + text;
            return await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id, text2, e.Message.Chat.Type,
                ParseMode.Html);
        }

        private static string GetMessageTo(MessageEventArgs e)
        {
            var name = e.Message.From.FirstName + " " + e.Message.From.LastName;
            name = name.Trim();
            return "<a href=\"tg://user?id=" + e.Message.From.Id + "\">" + name + "</a>";
        }

        internal static async Task<bool> SendMessageInPrivate(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
            string text, ParseMode html = ParseMode.Default)
        {
            try
            {
                return await telegramBotClient.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, html);
            }
            catch
            {
                return false;
            }
        }

        internal static async Task<bool> SendFileAsync(TelegramFile file, Tuple<TLAbsInputPeer, long> peer,
            string text, TextAsCaption textAsCaption, TelegramBotAbstract telegramBotAbstract, string username)
        {
            return await telegramBotAbstract.SendFileAsync(file, peer, text, textAsCaption, username);
        }

        public static async Task<TLAbsUpdates> SendMessageUserBot(TelegramClient userbotClient, 
            TLAbsInputPeer peer, string text, string username)
        {
            TLAbsUpdates r2 = null;
            try
            {
                r2 = await userbotClient.SendMessageAsync(peer, text);
            }
            catch
            {
                if (string.IsNullOrEmpty(username))
                {
                    return null;
                }
         
                var peerBetter = await UserbotPeer.GetPeerUserWithAccessHash(username, userbotClient);

                try
                {
                    r2 = await userbotClient.SendMessageAsync(peerBetter, text);
                }
                catch
                {
                    return null;
                }
                                
            }

            return r2;
        }
    }
}