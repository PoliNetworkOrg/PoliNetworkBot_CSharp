#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class SendMessage
{
    internal static async Task<MessageSentResult?> SendMessageInPrivateOrAGroup(
        TelegramBotAbstract? telegramBotClient,
        Language text, string? lang, string? username, long? userId, string? firstName, string? lastName, long? chatId,
        ChatType? chatType, ParseMode parseMode = ParseMode.Html, InlineKeyboardMarkup? inlineKeyboardMarkup = null)
    {
        MessageSentResult? r = null;
        try
        {
            if (telegramBotClient != null)
                r = await telegramBotClient.SendTextMessageAsync(userId,
                    text, ChatType.Private, parseMode: parseMode,
                    lang: lang, username: username,
                    replyMarkupObject: new ReplyMarkupObject(inlineKeyboardMarkup));
            if (r != null && r.IsSuccess()) return r;
        }
        catch
        {
            // ignored
        }

        if (!(r == null || r.IsSuccess() == false)) return r;

        var messageTo = GetMessageTo(firstName, lastName, userId);
        var text3 = new Language(new Dictionary<string, string?>
        {
            { "en", "[Message for " + messageTo + "]\n\n" + text.Select("en") },
            { "it", "[Messaggio per " + messageTo + "]\n\n" + text.Select("it") }
        });

        if (telegramBotClient != null)
            return await telegramBotClient.SendTextMessageAsync(chatId, text3, chatType,
                lang, parseMode, new ReplyMarkupObject(inlineKeyboardMarkup), username);
        return null;
    }

    private static string GetMessageTo(string? firstname, string? lastname, long? messageFromUserId)
    {
        var name = firstname + " " + lastname;
        name = name.Trim();
        return "<a href=\"tg://user?id=" + messageFromUserId + "\">" + name + "</a>";
    }

    internal static async Task<MessageSentResult?> SendMessageInPrivate(TelegramBotAbstract? telegramBotClient,
        long? userIdToSendTo, string? langCode, string? usernameToSendTo,
        Language? text, ParseMode parseMode, long? messageIdToReplyTo,
        InlineKeyboardMarkup? inlineKeyboardMarkup, EventArgsContainer eventArgsContainer, bool notifyOwners = true)
    {
        var stackTrace = Environment.StackTrace;

        try
        {
            if (telegramBotClient != null)
                return await telegramBotClient.SendTextMessageAsync(userIdToSendTo, text,
                    ChatType.Private, parseMode: parseMode,
                    lang: langCode, username: usernameToSendTo,
                    replyMarkupObject: new ReplyMarkupObject(inlineKeyboardMarkup),
                    replyToMessageId: messageIdToReplyTo);
        }
        catch (Exception e)
        {
            if (notifyOwners)
                await NotifyUtil.NotifyOwnersWithLog(e, telegramBotClient, stackTrace, eventArgsContainer);

            return new MessageSentResult(false, null, ChatType.Private);
        }

        return null;
    }

    internal static async Task<MessageSentResult?> SendMessageInAGroup(TelegramBotAbstract? telegramBotClient,
        string? lang, Language? text, EventArgsContainer? messageEventArgs,
        long chatId, ChatType chatType, ParseMode parseMode, long? replyToMessageId,
        bool disablePreviewLink, InlineKeyboardMarkup? inlineKeyboardMarkup = null)
    {
        MessageSentResult? r1 = null;

        if (telegramBotClient == null) return null;


        try
        {
            r1 = await telegramBotClient.SendTextMessageAsync(chatId,
                text,
                chatType,
                lang,
                parseMode,
                username: null,
                replyMarkupObject: new ReplyMarkupObject(inlineKeyboardMarkup),
                replyToMessageId: replyToMessageId,
                disablePreviewLink: disablePreviewLink,
                splitMessage: true);
        }
        catch (Exception? e1)
        {
            await NotifyUtil.NotifyOwnerWithLog25(e1, telegramBotClient, EventArgsContainer.Get(messageEventArgs));
        }

        return r1;
    }

    internal static bool SendFileAsync(TelegramFile file, PeerAbstract peer,
        Language? text, TextAsCaption textAsCaption, TelegramBotAbstract? telegramBotAbstract,
        string? username, string? lang, long? replyToMessageId, bool disablePreviewLink,
        ParseMode parseModeCaption = ParseMode.Html)
    {
        return telegramBotAbstract != null && telegramBotAbstract.SendFileAsync(file, peer, text, textAsCaption,
            username, lang,
            replyToMessageId, disablePreviewLink, parseModeCaption);
    }

    public static async Task<TLAbsUpdates?> SendMessageUserBot(TelegramClient? userbotClient,
        TLAbsInputPeer? peer, Language? text, string? username, TLAbsReplyMarkup? tlAbsReplyMarkup, string? lang,
        long? replyToMessageId, bool disablePreviewLink)
    {
        TLAbsUpdates? r2 = null;
        try
        {
            if (userbotClient != null)
                if (text != null)
                    r2 = await userbotClient.SendMessageAsync(peer, text.Select(lang), replyMarkup: tlAbsReplyMarkup);
        }
        catch
        {
            if (string.IsNullOrEmpty(username)) return null;

            var peerBetter = await UserbotPeer.GetPeerUserWithAccessHash(username, userbotClient);

            try
            {
                if (userbotClient != null)
                    r2 = await userbotClient.SendMessageAsync(peerBetter, text?.Select(lang),
                        replyMarkup: tlAbsReplyMarkup);
            }
            catch
            {
                return null;
            }
        }

        return r2;
    }

    public static SuccessQueue PlaceMessageInQueue(Message? replyTo, DateTimeSchedule? sentDate,
        long? messageFromIdPerson, long? messageFromIdEntity,
        long? idChatSentInto, TelegramBotAbstract? sender, ChatType? typeChatSentInto)
    {
        if (sentDate == null)
            return SuccessQueue.DATE_INVALID;

        if (replyTo == null)
            return SuccessQueue.INVALID_OBJECT;

        if (messageFromIdPerson == null)
            return SuccessQueue.INVALID_MESSAGE_FROM_ID_PERSON;

        var d1 = sentDate.IsInvalid();
        if (d1) return SuccessQueue.DATE_INVALID;

        if (replyTo.Photo != null)
        {
            var photoLarge = UtilsPhoto.GetLargest(replyTo.Photo);
            if (photoLarge == null) return SuccessQueue.INVALID_OBJECT;
            var photoIdDb = UtilsPhoto.AddPhotoToDb(photoLarge, sender);
            if (photoIdDb == null)
                return SuccessQueue.INVALID_ID_TO_DB;


            if (sender != null)
                MessageDb.AddMessage(MessageType.Photo,
                    replyTo.Caption, messageFromIdPerson.Value,
                    messageFromIdEntity,
                    idChatSentInto, sentDate.GetDate(), false,
                    sender.GetId(), replyTo.MessageId,
                    typeChatSentInto, photoIdDb.Value, null, sender);
        }
        else if (replyTo.Video != null)
        {
            var video = replyTo.Video;

            var videoMax = UtilsVideo.GetLargest(video);
            var videoIdDb = UtilsVideo.AddVideoToDb(videoMax, sender);
            if (videoIdDb == null)
                return SuccessQueue.INVALID_ID_TO_DB;

            if (sender != null)
                MessageDb.AddMessage(MessageType.Video,
                    replyTo.Caption, messageFromIdPerson.Value,
                    messageFromIdEntity,
                    idChatSentInto, sentDate.GetDate(), false,
                    sender.GetId(), replyTo.MessageId,
                    typeChatSentInto, null, videoIdDb.Value, sender);
        }
        else
        {
            return SuccessQueue.INVALID_OBJECT;
        }

        return SuccessQueue.SUCCESS;
    }


    public static async Task<bool> SendMessageInChannel2(MessageEventArgs? e, TelegramBotAbstract? sender,
        string[]? cmdLines)
    {
        if (e != null)
        {
            var message = e.Message;
            if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                message.Chat.Type == ChatType.Private)
            {
                if (cmdLines != null && (e.Message.ReplyToMessage == null || cmdLines.Length != 2))
                    return false;
                var text = new Language(new Dictionary<string, string?>
                {
                    { "it", e.Message.ReplyToMessage?.Text ?? e.Message.ReplyToMessage?.Caption }
                });
                var c2 = cmdLines?[1];
                if (cmdLines == null)
                    return false;

                if (c2 != null)
                    _ = await SendMessageInAGroup(sender, e.Message.From?.LanguageCode,
                        text, EventArgsContainer.Get(e),
                        long.Parse(c2),
                        ChatType.Channel, ParseMode.Html, null, false);

                return false;
            }
        }

        await CommandDispatcher.DefaultCommand(sender, e);

        return false;
    }
}