#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using SampleNuGet.Objects;
using SampleNuGet.Objects.Exceptions;
using SampleNuGet.Objects.TelegramMedia;
using SampleNuGet.Utils;
using SampleNuGet.Utils.UtilsMedia;
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
        ChatType? chatType, int? messageThreadId, ParseMode parseMode = ParseMode.Html,
        InlineKeyboardMarkup? inlineKeyboardMarkup = null)
    {
        MessageSentResult? r = null;
        var replyMarkupObject = new ReplyMarkupObject(inlineKeyboardMarkup);
        try
        {
            if (telegramBotClient != null)
                r = await telegramBotClient.SendTextMessageAsync(userId,
                    text, ChatType.Private, parseMode: parseMode,
                    lang: lang, username: username,
                    replyMarkupObject: replyMarkupObject, messageThreadId: messageThreadId);
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
                lang, parseMode, replyMarkupObject, username, messageThreadId);
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
        InlineKeyboardMarkup? inlineKeyboardMarkup, EventArgsContainer eventArgsContainer, int? messageThreadId,
        bool notifyOwners = true)
    {
        var stackTrace = Environment.StackTrace;

        try
        {
            if (telegramBotClient != null)
            {
                var replyMarkupObject = new ReplyMarkupObject(inlineKeyboardMarkup);
                return await telegramBotClient.SendTextMessageAsync(userIdToSendTo, text,
                    ChatType.Private, parseMode: parseMode,
                    lang: langCode, username: usernameToSendTo,
                    replyMarkupObject: replyMarkupObject,
                    replyToMessageId: messageIdToReplyTo,
                    messageThreadId: messageThreadId);
            }
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
        bool disablePreviewLink,
        int? messageThreadId,
        InlineKeyboardMarkup? inlineKeyboardMarkup = null)
    {
        MessageSentResult? r1 = null;

        if (telegramBotClient == null) return null;


        try
        {
            var replyMarkupObject = new ReplyMarkupObject(inlineKeyboardMarkup);
            r1 = await telegramBotClient.SendTextMessageAsync(chatId,
                text,
                chatType,
                lang,
                parseMode,
                username: null,
                replyMarkupObject: replyMarkupObject,
                replyToMessageId: replyToMessageId,
                disablePreviewLink: disablePreviewLink,
                splitMessage: true,
                messageThreadId: messageThreadId);
        }
        catch (Exception? e1)
        {
            await NotifyUtil.NotifyOwnerWithLog25(e1, telegramBotClient, EventArgsContainer.Get(messageEventArgs));
        }

        return r1;
    }

    internal static bool SendFileAsync(TelegramFile file, PeerAbstract peer,
        TelegramBotAbstract? telegramBotAbstract,
        string? username, int? messageThreadId, string? lang, long? replyToMessageId, bool disablePreviewLink,
        ParseMode parseModeCaption = ParseMode.Html)
    {
        return telegramBotAbstract != null &&
               telegramBotAbstract.SendFileAsync(
                   file, peer,
                   username, lang,
                   replyToMessageId, disablePreviewLink,
                   parseModeCaption: parseModeCaption,
                   messageThreadId: messageThreadId
               );
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


    public static void SendMessageInChannel2(ActionFuncGenericParams actionFuncGenericParams)
    {
        var e = actionFuncGenericParams.MessageEventArgs;
        var cmdLines = actionFuncGenericParams.Strings;
        var sender = actionFuncGenericParams.TelegramBotAbstract;
        if (e == null || cmdLines == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        var eMessage = e.Message;
        if (!Owners.CheckIfOwner(eMessage.From?.Id) || eMessage.Chat.Type != ChatType.Private)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        if (eMessage.ReplyToMessage == null || cmdLines.Length != 2)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        var text = new Language(new Dictionary<string, string?>
        {
            { "it", eMessage.ReplyToMessage?.Text ?? eMessage.ReplyToMessage?.Caption }
        });
        var c2 = cmdLines[1];

        var sendMessageInAGroup = SendMessageInAGroup(sender, eMessage.From?.LanguageCode,
            text, EventArgsContainer.Get(e),
            long.Parse(c2),
            ChatType.Channel, ParseMode.Html, null, false, eMessage.MessageThreadId);

        sendMessageInAGroup.Wait();
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<SuccessWithException> ForwardMessage(TelegramBotAbstract? sender, MessageEventArgs? e,
        ChatId chatId, ChatId fromChatId, int messageId, int? messageThreadId,
        bool? disableNotification = default, bool? protectContent = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (sender == null) return new SuccessWithException(false);
            await sender.ForwardMessageAsync(chatId, fromChatId, messageId,
                disableNotification, protectContent, messageThreadId, cancellationToken);
            return new SuccessWithException(true);
        }
        catch (Exception ex)
        {
            return new SuccessWithException(true, ex);
        }
    }
}