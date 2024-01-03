#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            {
                var messageOptions = new TelegramBotAbstract.MessageOptions

                {
                    ChatId = userId,
                    Text = text,
                    ParseMode = parseMode,
                    Lang = lang,
                    Username = username,
                    ReplyMarkupObject = new ReplyMarkupObject(inlineKeyboardMarkup),
                    ChatType = ChatType.Private
                };
                r = await telegramBotClient.SendTextMessageAsync(messageOptions);
            }

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
        {
            var messageOptions = new TelegramBotAbstract.MessageOptions

            {
                ChatId = chatId,
                Text = text3,
                ParseMode = parseMode,
                Lang = lang,
                Username = username,
                ReplyMarkupObject = new ReplyMarkupObject(inlineKeyboardMarkup),
                ChatType = ChatType.Private
            };
            var sendTextMessageAsync = await telegramBotClient.SendTextMessageAsync(messageOptions);
            return sendTextMessageAsync;
        }

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
            {
                var messageOptions = new TelegramBotAbstract.MessageOptions

                {
                    ChatId = userIdToSendTo,
                    Text = text,
                    ParseMode = parseMode,
                    Lang = langCode,
                    Username = usernameToSendTo,
                    ReplyMarkupObject = new ReplyMarkupObject(inlineKeyboardMarkup),
                    ChatType = ChatType.Private,
                    ReplyToMessageId = messageIdToReplyTo
                };
                return await telegramBotClient.SendTextMessageAsync(messageOptions);
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
        bool disablePreviewLink, InlineKeyboardMarkup? inlineKeyboardMarkup = null)
    {
        MessageSentResult? r1 = null;

        if (telegramBotClient == null) return null;


        try
        {
            var messageOptions = new TelegramBotAbstract.MessageOptions

            {
                ChatId = chatId,
                Text = text,
                ParseMode = parseMode,
                Lang = lang,

                ReplyMarkupObject = new ReplyMarkupObject(inlineKeyboardMarkup),
                ChatType = chatType,
                ReplyToMessageId = replyToMessageId,
                DisablePreviewLink = disablePreviewLink,
                SplitMessage = true
            };

            r1 = await telegramBotClient.SendTextMessageAsync(messageOptions);
        }
        catch (Exception? e1)
        {
            await NotifyUtil.NotifyOwnerWithLog25(e1, telegramBotClient, EventArgsContainer.Get(messageEventArgs));
        }

        return r1;
    }

    internal static bool SendFileAsync(TelegramFile file, PeerAbstract peer,
        TelegramBotAbstract? telegramBotAbstract,
        string? username, string? lang, long? replyToMessageId, bool disablePreviewLink,
        ParseMode parseModeCaption = ParseMode.Html)
    {
        var messageOptions = new TelegramBotAbstract.MessageOptions

        {
            documentInput = file,
            peer = peer,
            ChatId = peer.GetUserId(),
            Username = username,
            Lang = lang,
            ReplyToMessageId = replyToMessageId,
            DisablePreviewLink = disablePreviewLink,
            ParseMode = parseModeCaption
        };
        return telegramBotAbstract != null &&
               telegramBotAbstract.SendFileAsync(messageOptions);
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


    public static async Task<CommandExecutionState> SendMessageInChannel2(MessageEventArgs? e,
        TelegramBotAbstract? sender,
        string[]? cmdLines)
    {
        if (e == null || cmdLines == null) return CommandExecutionState.UNMET_CONDITIONS;
        var message = e.Message;
        if (!Owners.CheckIfOwner(e.Message.From?.Id) || message.Chat.Type != ChatType.Private)
            return CommandExecutionState.UNMET_CONDITIONS;
        if (e.Message.ReplyToMessage == null || cmdLines.Length != 2)
            return CommandExecutionState.UNMET_CONDITIONS;
        var text = new Language(new Dictionary<string, string?>
        {
            { "it", e.Message.ReplyToMessage?.Text ?? e.Message.ReplyToMessage?.Caption }
        });
        var c2 = cmdLines[1];

        await SendMessageInAGroup(sender, e.Message.From?.LanguageCode,
            text, EventArgsContainer.Get(e),
            long.Parse(c2),
            ChatType.Channel, ParseMode.Html, null, false);

        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<SuccessWithException> ForwardMessage(TelegramBotAbstract? sender, MessageEventArgs? e,
        ChatId chatId, ChatId fromChatId, int messageId,
        bool? disableNotification = default, bool? protectContent = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (sender == null) return new SuccessWithException(false);
            await sender.ForwardMessageAsync(chatId, fromChatId, messageId, disableNotification, protectContent,
                cancellationToken);
            return new SuccessWithException(true);
        }
        catch (Exception ex)
        {
            return new SuccessWithException(true, ex);
        }
    }
}