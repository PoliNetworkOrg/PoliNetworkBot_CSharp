#region

using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class SendMessage
    {
        internal static async Task<Code.Objects.MessageSend> SendMessageInPrivateOrAGroup(TelegramBotAbstract telegramBotClient,
            Language text, string lang, string username, int userId, string firstName, string lastName, long chatId,
            ChatType chatType, ParseMode parseMode = ParseMode.Html)
        {
            MessageSend r = null;
            try
            {
                r = await telegramBotClient.SendTextMessageAsync(userId,
                    text, ChatType.Private, parseMode: parseMode,
                    lang: lang, username: username,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE));
                if (r.IsSuccess())
                {
                    return r;
                }
            }
            catch
            {
                // ignored
            }

            if (!(r == null || r.IsSuccess() == false))
            {
                return r;
            }

            var messageTo = GetMessageTo(firstName, lastName, userId);
            var text3 = new Language(new Dictionary<string, string>
            {
                {"en", "[Message for " + messageTo + "]\n\n" + text.Select("en")},
                {"it", "[Messaggio per " + messageTo + "]\n\n" + text.Select("it")}
            });

            return await telegramBotClient.SendTextMessageAsync(chatId, text3, chatType,
                lang, parseMode, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), username);
        }

        private static string GetMessageTo(string firstname, string lastname, long messageFromUserId)
        {
            var name = firstname + " " + lastname;
            name = name.Trim();
            return "<a href=\"tg://user?id=" + messageFromUserId + "\">" + name + "</a>";
        }

        internal static async Task<MessageSend> SendMessageInPrivate(TelegramBotAbstract telegramBotClient,
            long userIdToSendTo, string langCode, string usernameToSendTo,
            Language text, ParseMode parseMode, long? messageIdToReplyTo)
        {
            try
            {
                return await telegramBotClient.SendTextMessageAsync(userIdToSendTo, text,
                    ChatType.Private, parseMode: parseMode,
                    lang: langCode, username: usernameToSendTo,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), replyToMessageId: messageIdToReplyTo);
            }
            catch
            {
                return new MessageSend(false, null, ChatType.Private);
            }
        }

        internal static async Task<MessageSend> SendMessageInAGroup(TelegramBotAbstract telegramBotClient,
            string lang, Language text,
            long chatId, ChatType chatType, ParseMode parseMode, long? replyToMessageId,
            bool disablePreviewLink)
        {
            MessageSend r1 = null;
            try
            {
                r1 = await telegramBotClient.SendTextMessageAsync(chatId, text, chatType, lang, parseMode, null, null, replyToMessageId, disablePreviewLink);
            }
            catch (Exception e1)
            {
                ;
            }

            return r1;
        }

        internal static async Task<bool> SendFileAsync(TelegramFile file, Tuple<TLAbsInputPeer, long> peer,
            Language text, TextAsCaption textAsCaption, TelegramBotAbstract telegramBotAbstract,
            string username, string lang, long? replyToMessageId, bool disablePreviewLink)
        {
            return await telegramBotAbstract.SendFileAsync(file, peer, text, textAsCaption, username, lang, replyToMessageId: replyToMessageId, disablePreviewLink);
        }

        public static async Task<TLAbsUpdates> SendMessageUserBot(TelegramClient userbotClient,
            TLAbsInputPeer peer, Language text, string username, TLAbsReplyMarkup tlAbsReplyMarkup, string lang, long? replyToMessageId, bool disablePreviewLink)
        {
            TLAbsUpdates r2;
            try
            {
                r2 = await userbotClient.SendMessageAsync(peer, text.Select(lang), replyMarkup: tlAbsReplyMarkup);
            }
            catch
            {
                if (string.IsNullOrEmpty(username)) return null;

                var peerBetter = await UserbotPeer.GetPeerUserWithAccessHash(username, userbotClient);

                try
                {
                    r2 = await userbotClient.SendMessageAsync(peerBetter, text.Select(lang), replyMarkup: tlAbsReplyMarkup);
                }
                catch
                {
                    return null;
                }
            }

            return r2;
        }

        public static SuccessQueue PlaceMessageInQueue(Message replyTo, DateTimeSchedule sentDate,
            int messageFromIdPerson, int? messageFromIdEntity,
            long idChatSentInto, TelegramBotAbstract sender, ChatType typeChatSentInto)
        {
            if (sentDate == null)
                return SuccessQueue.DATE_INVALID;

            if (replyTo == null)
                return SuccessQueue.INVALID_OBJECT;

            var d1 = sentDate.IsInvalid();
            if (d1) return SuccessQueue.DATE_INVALID;

            if (replyTo.Photo != null)
            {
                var photoLarge = UtilsPhoto.GetLargest(replyTo.Photo);
                if (photoLarge == null)
                {
                    return SuccessQueue.INVALID_OBJECT;
                }
                var photoIdDb = UtilsPhoto.AddPhotoToDb(photoLarge);
                if (photoIdDb == null)
                    return SuccessQueue.INVALID_ID_TO_DB;

                MessageDb.AddMessage(MessageType.Photo,
                    replyTo.Caption, messageFromIdPerson,
                    messageFromIdEntity,
                    idChatSentInto, sentDate.GetDate(), false,
                    (int)sender.GetId(), replyTo.MessageId,
                    typeChatSentInto, photoIdDb.Value, null);
            }
            else if (replyTo.Video != null)
            {
                ;
                var video = replyTo.Video;

                var videoMax = UtilsVideo.GetLargest(video);
                var videoIdDb = UtilsVideo.AddVideoToDb(videoMax);
                if (videoIdDb == null)
                    return SuccessQueue.INVALID_ID_TO_DB;

                MessageDb.AddMessage(MessageType.Video,
                    replyTo.Caption, messageFromIdPerson,
                    messageFromIdEntity,
                    idChatSentInto, sentDate.GetDate(), false,
                    (int)sender.GetId(), replyTo.MessageId,
                    typeChatSentInto, null, videoIdDb.Value);
            }
            else
            {
                return SuccessQueue.INVALID_OBJECT;
            }

            return SuccessQueue.SUCCESS;
        }
    }
}