#region

using System;
using System.Collections.Generic;
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
            Language text, string lang, string username)
        {
            try
            {
                var r = await telegramBotClient.SendTextMessageAsync(chatid: e.Message.From.Id,
                    text: text, chatType: ChatType.Private, parseMode: ParseMode.Html, 
                    lang: lang, username: username,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE) );
                if (r) return true;
            }
            catch
            {
                // ignored
            }

            var messageTo = GetMessageTo(e);
            var language = e.Message.From.LanguageCode.ToLower();
            var text3 = new Language(dict: new Dictionary<string, string>()
            {
                {"en" , "[Message for " + messageTo + "]\n\n" + text.Select("en")},
                {"it" , "[Messaggio per " + messageTo + "]\n\n" + text.Select("it")},
            });

            
            return await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id, text3, e.Message.Chat.Type,
                 lang: lang,  parseMode: ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), username: username );
        }

        private static string GetMessageTo(MessageEventArgs e)
        {
            var name = e.Message.From.FirstName + " " + e.Message.From.LastName;
            name = name.Trim();
            return "<a href=\"tg://user?id=" + e.Message.From.Id + "\">" + name + "</a>";
        }

        internal static async Task<bool> SendMessageInPrivate(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
            Language text, ParseMode html = ParseMode.Default)
        {
            try
            {
                return await telegramBotClient.SendTextMessageAsync(chatid: e.Message.From.Id, text: text, 
                    chatType:ChatType.Private, parseMode: html, 
                    lang: e.Message.From.LanguageCode, username: e.Message.From.Username,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE));
            }
            catch
            {
                return false;
            }
        }

        internal static async Task<bool> SendFileAsync(TelegramFile file, Tuple<TLAbsInputPeer, long> peer,
            Language text, TextAsCaption textAsCaption, TelegramBotAbstract telegramBotAbstract,
            string username, string lang)
        {
            return await telegramBotAbstract.SendFileAsync(file, peer, text, textAsCaption, username, lang);
        }

        public static async Task<TLAbsUpdates> SendMessageUserBot(TelegramClient userbotClient,
            TLAbsInputPeer peer, Language text, string username, TLAbsReplyMarkup tlAbsReplyMarkup, string lang)
        {
            TLAbsUpdates r2 = null;
            try
            {
                r2 = await userbotClient.SendMessageAsync(peer, text.Select(lang));
            }
            catch
            {
                if (string.IsNullOrEmpty(username)) return null;

                var peerBetter = await UserbotPeer.GetPeerUserWithAccessHash(username, userbotClient);

                try
                {
                    r2 = await userbotClient.SendMessageAsync(peerBetter, text.Select(lang));
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