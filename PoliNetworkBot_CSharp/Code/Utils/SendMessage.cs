﻿#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
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
        InlineKeyboardMarkup? inlineKeyboardMarkup = null)
    {
        try
        {
            if (telegramBotClient != null)
                return await telegramBotClient.SendTextMessageAsync(userIdToSendTo, text,
                    ChatType.Private, parseMode: parseMode,
                    lang: langCode, username: usernameToSendTo,
                    replyMarkupObject: new ReplyMarkupObject(inlineKeyboardMarkup),
                    replyToMessageId: messageIdToReplyTo);
        }
        catch
        {
            return new MessageSentResult(false, null, ChatType.Private);
        }

        return null;
    }

    internal static async Task<MessageSentResult?> SendMessageInAGroup(TelegramBotAbstract? telegramBotClient,
        string? lang, Language? text, MessageEventArgs? messageEventArgs,
        long chatId, ChatType chatType, ParseMode parseMode, long? replyToMessageId,
        bool disablePreviewLink, int i = 0, InlineKeyboardMarkup? inlineKeyboardMarkup = null)
    {
        MessageSentResult? r1 = null;

        if (telegramBotClient == null) return null;

        if (i > 5)
            return null;

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
            await NotifyUtil.NotifyOwnerWithLog2(e1, telegramBotClient, messageEventArgs);
        }

        return r1;
    }

    internal static async Task<bool> SendFileAsync(TelegramFile file, PeerAbstract peer,
        Language? text, TextAsCaption textAsCaption, TelegramBotAbstract? telegramBotAbstract,
        string? username, string? lang, long? replyToMessageId, bool disablePreviewLink,
        ParseMode parseModeCaption = ParseMode.Html)
    {
        return telegramBotAbstract != null && await telegramBotAbstract.SendFileAsync(file, peer, text, textAsCaption,
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
}