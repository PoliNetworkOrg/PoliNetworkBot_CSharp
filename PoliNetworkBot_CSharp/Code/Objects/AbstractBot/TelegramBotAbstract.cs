﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;
using File = Telegram.Bot.Types.File;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.AbstractBot;

[Serializable]
public class TelegramBotAbstract
{
    private readonly TelegramBotClient? _botClient;
    private readonly string? _contactString;

    private readonly long? _id;
    private readonly BotTypeApi _isbot;
    private readonly string? _mode;

    private readonly string? _website;


    public readonly TelegramClient? UserbotClient;
    private string? _username;
    public BotInfoAbstract? BotInfoAbstract;

    public DbConfigConnection? DbConfig;
    public string? GithubToken;

    private TelegramBotAbstract(TelegramBotClient? botClient, TelegramClient? userBotClient, BotTypeApi? botTypeApi,
        string? website, string? contactString, long? id, string? githubToken, BotInfoAbstract? botInfoAbstract)


    {
        UserbotClient = userBotClient;
        _botClient = botClient;
        _isbot = botTypeApi ?? BotTypeApi.UNKNOWN;
        _website = website;
        _contactString = contactString;
        _id = id;

        BotInfoAbstract = botInfoAbstract;

        GithubToken = githubToken;
    }

    public TelegramBotAbstract(TelegramBotClient? botClient, string? website, string? contactString,
        BotTypeApi? botTypeApi, string? mode, string? githubToken, BotInfoAbstract botInfoAbstract)
        : this(botClient, null, botTypeApi, website,
            contactString,
            botClient?.BotId, githubToken, botInfoAbstract)


    {
        _mode = mode;
    }

    public TelegramBotAbstract(TelegramClient? userbotClient, string? website, string? contactString, long? id,
        BotTypeApi? botTypeApi, string? mode, string? githubToken, BotInfoAbstract botInfoAbstract) :
        this(null, userbotClient, botTypeApi, website, contactString, id, githubToken, botInfoAbstract)


    {
        _mode = mode;
    }


    public TelegramBotAbstract(TelegramBotClient? botClient, BotInfoAbstract? botInfoAbstract)
        : this(botClient, null, BotTypeApi.REAL_BOT, null, null,
            null, null, botInfoAbstract)

    {
    }

    internal async Task ExitGroupAsync(MessageEventArgs? e)
    {
        try
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                {
                    if (_botClient != null)
                        if (e is { Message: not null })
                            await _botClient.LeaveChatAsync(e.Message.Chat.Id);
                }
                    break;

                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;
            }
        }
        catch (Exception? ex)
        {
            await NotifyUtil.NotifyOwnerWithLog2(ex, this, EventArgsContainer.Get(e));
        }
    }

    internal string? GetMode()
    {
        return _mode;
    }

    public async Task<TLAbsUpdates?> AddUserIntoChannel(string userId, TLChannel channel)
    {
        if (string.IsNullOrEmpty(userId))
            return null;

        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                break;

            case BotTypeApi.USER_BOT:
            {
                try
                {
                    var users = new TLVector<TLAbsInputUser?>();
                    if (userId.StartsWith("@"))
                    {
                        var u = await UserbotPeer.GetPeerUserWithAccessHash(userId[1..], UserbotClient);
                        if (u != null)
                        {
                            TLAbsInputUser input2 = new TLInputUser { AccessHash = u.AccessHash, UserId = u.UserId };
                            users.Add(input2);
                        }
                    }
                    else
                    {
                        users.Add(UserbotPeer.GetPeerUserFromId(Convert.ToInt64(userId)));
                    }

                    var tLInputChannel = new TLInputChannel { ChannelId = channel.Id };
                    if (channel.AccessHash != null)
                        tLInputChannel.AccessHash = channel.AccessHash.Value;

                    if (UserbotClient != null)
                    {
                        var r = await UserbotClient.ChannelsInviteToChannel(tLInputChannel, users);
                        return r;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;
        }

        return null;
    }

    internal async Task<TlChannelClass?> UpgradeGroupIntoSupergroup(long? chatId)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                break;

            case BotTypeApi.USER_BOT:
            {
                if (UserbotClient != null)
                {
                    var r = await UserbotClient.UpgradeGroupIntoSupergroup(chatId);
                    if (r is TLUpdates { Chats.Count: 2 } r2)
                    {
                        var c1 = r2.Chats[1];
                        if (c1 is TLChannel c2) return new TlChannelClass(c2);
                    }
                }
            }
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;
        }

        return null;
    }

    public async Task<bool?> EditDescriptionChannel(TLChannel channel, string desc)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                break;

            case BotTypeApi.USER_BOT:
            {
                if (UserbotClient != null)
                {
                    var r = await UserbotClient.Channels_EditDescription(channel, desc);
                    return r;
                }

                break;
            }

            case BotTypeApi.DISGUISED_BOT:
                break;
        }

        return null;
    }

    internal async Task<string?> GetBotUsernameAsync()
    {
        if (!string.IsNullOrEmpty(_username))
            return _username;

        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                if (_botClient == null) return _username;
                var x = await _botClient.GetMeAsync();
                var u1 = x.Username;
                if (u1 != null && u1.StartsWith("@"))
                    u1 = u1[1..];

                _username = u1;

                return _username;
            }

            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;
        }

        return null;
    }

    internal async Task<Tuple<Chat?, Exception?>?> GetChat(long chatId)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                Exception? e = null;
                try
                {
                    if (_botClient != null)
                        return new Tuple<Chat?, Exception?>(await _botClient.GetChatAsync(chatId), null);
                }
                catch (Exception? e2)
                {
                    e = e2;
                }

                if (chatId <= 0) return new Tuple<Chat?, Exception?>(null, e);
                await Task.Delay(100);

                var chatidS = chatId.ToString();
                chatidS = "-100" + chatidS;
                var chatidSl = Convert.ToInt64(chatidS);
                try
                {
                    if (_botClient != null)
                        return new Tuple<Chat?, Exception?>(await _botClient.GetChatAsync(chatidSl), e);
                }
                catch (Exception e3)
                {
                    return new Tuple<Chat?, Exception?>(null, e3);
                }

                break;
            }
            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    internal BotTypeApi GetBotType()
    {
        return _isbot;
    }

    internal string? GetWebSite()
    {
        return _website;
    }

    internal static TelegramBotAbstract? GetFromRam(TelegramBotClient? telegramBotClientBot)
    {
        return telegramBotClientBot?.BotId == null ? null : GlobalVariables.Bots?[telegramBotClientBot.BotId.Value];
    }

    internal async Task<bool> DeleteMessageAsync(long chatId, long messageId, long? accessHash)
    {
        //todo: rimettere e aggiungere una caption a questo metodo/log
        //Logger.WriteLogComplete(new List<object?> { chatId, messageId, accessHash }, this, "DeleteMessageAsync");

        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                try
                {
                    if (_botClient != null) await _botClient.DeleteMessageAsync(chatId, (int)messageId);
                }
                catch
                {
                    return false;
                }

                return true;
            }

            case BotTypeApi.USER_BOT:
            {
                var peer = UserbotPeer.GetPeerChannelFromIdAndType(chatId, accessHash);

                if (UserbotClient != null)
                {
                    var r1 = await UserbotClient.ChannelsDeleteMessageAsync(peer,
                        new TLVector<int> { (int)messageId });

                    return r1 != null;
                }

                break;
            }
            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    internal async Task<bool> PromoteChatMember(TLInputUser userIdInput, ChatId chatId, long? accessHashChat)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                try
                {
                    if (_botClient != null)
                        await _botClient.PromoteChatMemberAsync(chatId, userIdInput.UserId, true, true, true, true,
                            true, true, true, true);
                }
                catch (Exception? e)
                {
                    await NotifyUtil.NotifyOwnerWithLog2(e, this, null);
                    return false;
                }

                return true;
            }

            case BotTypeApi.USER_BOT:
            {
                try
                {
                    TLAbsChannelParticipantRole role = new TLChannelRoleEditor();

                    if (UserbotClient != null)
                        await UserbotClient.ChannelsEditAdmin(
                            UserbotPeer.GetPeerChannelFromIdAndType(chatId.Identifier, accessHashChat),
                            userIdInput,
                            role);
                }
                catch (Exception? e)
                {
                    await NotifyUtil.NotifyOwnerWithLog2(e, this, null);
                    return false;
                }

                break;
            }

            case BotTypeApi.DISGUISED_BOT:
                break;
        }

        return false;
    }

    internal async Task<UserIdFound?> GetIdFromUsernameAsync(string? target)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                var userBot = FindFirstUserBot();
                if (userBot == null)
                    return new UserIdFound(null, "BotApiDoesNotAllowThat");
                return await userBot.GetIdFromUsernameAsync(target);
            }

            case BotTypeApi.USER_BOT:
                if (UserbotClient != null)
                {
                    var r = await UserbotClient.ResolveUsernameAsync(target);
                    return r.Peer switch
                    {
                        null => new UserIdFound(null, "UserbotCantFindTheIDofTarget(1)"),
                        TLPeerUser tLPeerUser => new UserIdFound(tLPeerUser.UserId, "UserbotCantFindTheIDofTarget(2)"),
                        _ => new UserIdFound(null, "UserbotCantFindTheIDofTarget(3)")
                    };
                }

                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new UserIdFound(null, "BotIsNotOfAnyType");
    }

    private static TelegramBotAbstract? FindFirstUserBot()
    {
        if (GlobalVariables.Bots == null) return null;
        foreach (var bot in GlobalVariables.Bots.Keys.Select(b => GlobalVariables.Bots[b]))
            if (bot != null)
                switch (bot._isbot)
                {
                    case BotTypeApi.REAL_BOT:
                        break;

                    case BotTypeApi.USER_BOT:
                        return bot;

                    case BotTypeApi.DISGUISED_BOT:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

        return null;
    }

    internal async Task<MessageSentResult?> ForwardMessageAsync(int messageId, long idChatMessageFrom,
        long? idChatMessageTo)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                if (_botClient != null)
                    if (idChatMessageTo != null)
                    {
                        var m = await _botClient.ForwardMessageAsync(idChatMessageTo, idChatMessageFrom, messageId);
                        return new MessageSentResult(true, m, m.Chat.Type);
                    }

                break;
            }
            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;
        }

        return null;
    }

    internal async Task RestrictChatMemberAsync(long chatId, long? userId, ChatPermissions permissions,
        DateTime? untilDate, ChatType? chatType)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                switch (chatType)
                {
                    case ChatType.Supergroup:
                    {
                        if (userId != null)
                            if (_botClient != null)
                                await _botClient.RestrictChatMemberAsync(
                                    chatId, userId.Value,
                                    permissions,
                                    null,
                                    untilDate);

                        break;
                    }

                    case ChatType.Group:
                    {
                        Logger.WriteLine("Can't restrict a user in a group");
                        break;
                    }
                    case ChatType.Private:
                        break;

                    case ChatType.Channel:
                        break;

                    case ChatType.Sender:
                        break;

                    case null:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(chatType), chatType, null);
                }

                break;
            }
            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal async Task<Tuple<File, Stream>?> DownloadFileAsync(Document d)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                var stream = new MemoryStream();
                if (_botClient != null)
                {
                    var f = await _botClient.GetInfoAndDownloadFileAsync(d.FileId, stream);

                    return new Tuple<File, Stream>(f, stream);
                }

                break;
            }
            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    internal async Task<Message?> EditText(ChatId chatId, int messageId, string newText,
        InlineKeyboardMarkup inlineKeyboardMarkup)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                if (_botClient != null)
                    return await _botClient.EditMessageTextAsync(chatId, messageId, newText,
                        replyMarkup: inlineKeyboardMarkup);
                break;
            }

            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    internal async Task RemoveInlineKeyboardAsync(ChatId chatId, long messageId, InlineKeyboardMarkup replyMarkup)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                if (_botClient != null)
                    await _botClient.EditMessageReplyMarkupAsync(chatId, (int)messageId, replyMarkup);
                return;
            }

            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal long? GetId()
    {
        return _isbot switch
        {
            BotTypeApi.REAL_BOT => _botClient?.BotId,
            BotTypeApi.USER_BOT => _id,
            BotTypeApi.DISGUISED_BOT => _id,
            BotTypeApi.UNKNOWN => _id,
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    /// <summary>
    ///     Send text message
    /// </summary>
    /// <param name="messageOptions"></param>
    /// Object with all the params inside
    /// <returns>MessageSentResult of the last message sent</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal async Task<MessageSentResult?> SendTextMessageAsync(MessageOptions messageOptions)
    {
        var message = messageOptions.Text?.Select(messageOptions.Lang);
        if (_botClient == null) return null;
        if (message == null) return null;

        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                IReplyMarkup? reply = null;
                if (messageOptions.ReplyMarkupObject != null)
                    reply = messageOptions.ReplyMarkupObject.GetReplyMarkupBot();
                var m2 = messageOptions.ReplyToMessageId;
                var messageOptionsParseMode = messageOptions.ParseMode ?? ParseMode.Html;
                var i = (int?)m2;

                while (messageOptions.SplitMessage && message.Length > 4096)
                {
                    if (messageOptions.ChatId != null)
                        if (_botClient != null)
                            await _botClient.SendTextMessageAsync(messageOptions.ChatId, message[..4095],
                                messageOptions.MessageThreadId,
                                messageOptionsParseMode,
                                replyMarkup: reply, replyToMessageId: i,
                                disableWebPagePreview: messageOptions.DisablePreviewLink);
                    message = message[4095..];
                    Thread.Sleep(100);
                }

                if (_botClient == null) return null;
                if (messageOptions.ChatId == null) return null;

                var textMessageAsync = await _botClient.SendTextMessageAsync(
                    messageOptions.ChatId,
                    message,
                    messageOptions.MessageThreadId,
                    messageOptionsParseMode,
                    replyMarkup: reply,
                    replyToMessageId: i,
                    disableWebPagePreview: messageOptions.DisablePreviewLink
                );

                var sendTextMessageAsync = new MessageSentResult(true, textMessageAsync,
                    messageOptions.ChatType);
                return sendTextMessageAsync;


            case BotTypeApi.USER_BOT:
            case BotTypeApi.DISGUISED_BOT:
                if (messageOptions.ChatId == null)
                    return null;
                var peer = UserbotPeer.GetPeerFromIdAndType(messageOptions.ChatId.Value, messageOptions.ChatType);
                try
                {
                    TLAbsReplyMarkup? replyMarkup = null;
                    if (messageOptions.ReplyMarkupObject != null)
                        replyMarkup = messageOptions.ReplyMarkupObject.GetReplyMarkupUserBot();
                    var m3 = await SendMessage.SendMessageUserBot(UserbotClient,
                        peer, messageOptions.Text, messageOptions.Username, replyMarkup, messageOptions.Lang,
                        messageOptions.ReplyToMessageId, messageOptions.DisablePreviewLink);
                    var b3 = m3 != null;
                    return new MessageSentResult(b3, m3, messageOptions.ChatType);
                }
                catch (Exception? e)
                {
                    Logger.WriteLine(e);
                }

                return new MessageSentResult(false, null, messageOptions.ChatType);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal async Task<bool> SendMedia(GenericFile genericFile, long chatid, ChatType chatType, string? username,
        Language? caption, string? lang)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                var messageType = genericFile.GetMediaBotType();
                switch (messageType)
                {
                    case MessageType.Unknown:
                        break;

                    case MessageType.Text:
                        break;

                    case MessageType.Photo:
                        break;

                    case MessageType.Audio:
                        break;

                    case MessageType.Video:
                        break;

                    case MessageType.Voice:
                        break;

                    case MessageType.Document:
                        break;

                    case MessageType.Sticker:
                        break;

                    case MessageType.Location:
                        break;

                    case MessageType.Contact:
                        break;

                    case MessageType.Venue:
                        break;

                    case MessageType.Game:
                        break;

                    case MessageType.VideoNote:
                        break;

                    case MessageType.Invoice:
                        break;

                    case MessageType.SuccessfulPayment:
                        break;

                    case MessageType.WebsiteConnected:
                        break;

                    case MessageType.ChatMembersAdded:
                        break;

                    case MessageType.ChatMemberLeft:
                        break;

                    case MessageType.ChatTitleChanged:
                        break;

                    case MessageType.ChatPhotoChanged:
                        break;

                    case MessageType.MessagePinned:
                        break;

                    case MessageType.ChatPhotoDeleted:
                        break;

                    case MessageType.GroupCreated:
                        break;

                    case MessageType.SupergroupCreated:
                        break;

                    case MessageType.ChannelCreated:
                        break;

                    case MessageType.MigratedToSupergroup:
                        break;

                    case MessageType.MigratedFromGroup:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            }
            case BotTypeApi.USER_BOT:
            {
                var peer = UserbotPeer.GetPeerFromIdAndType(chatid, chatType);
                var media2 = await genericFile.GetMediaTl(UserbotClient);

                if (media2 != null)
                {
                    var r = await media2.SendMedia(peer, UserbotClient, caption, username, lang);
                    return r != null;
                }

                break;
            }
            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    internal async Task<MessageSentResult?> ForwardMessageAnonAsync(long chatIdToSend, Message? message,
        int? messageIdToReplyToLong, int? messageThreadId)
    {
        if (message == null) return null;
        switch (message.Type)
        {
            case MessageType.Unknown:
                break;

            case MessageType.Text:
            {
                switch (_isbot)
                {
                    case BotTypeApi.REAL_BOT:
                    {
                        if (message.Text != null)
                            if (_botClient != null)
                            {
                                var m1 = await _botClient.SendTextMessageAsync(chatIdToSend, message.Text,
                                    messageThreadId, ParseMode.Html, replyToMessageId: messageIdToReplyToLong);
                                return new MessageSentResult(true, m1, m1.Chat.Type);
                            }

                        break;
                    }
                    case BotTypeApi.USER_BOT:
                        break;

                    case BotTypeApi.DISGUISED_BOT:
                        break;
                }

                break;
            }
            case MessageType.Photo:
            {
                switch (_isbot)
                {
                    case BotTypeApi.REAL_BOT:
                    {
                        if (_botClient != null)
                        {
                            var p1 = InputOnlineFile(message);
                            if (p1 != null)
                            {
                                var m1 = await _botClient.SendPhotoAsync(chatIdToSend, p1,
                                    messageThreadId, message.Caption,
                                    ParseMode.Html, replyToMessageId: messageIdToReplyToLong);
                                return new MessageSentResult(true, m1, m1.Chat.Type);
                            }
                        }

                        break;
                    }
                    case BotTypeApi.USER_BOT:
                        break;

                    case BotTypeApi.DISGUISED_BOT:
                        break;
                }

                break;
            }
            case MessageType.Audio:
                break;

            case MessageType.Video:
            {
                switch (_isbot)
                {
                    case BotTypeApi.REAL_BOT:
                    {
                        if (message.Video == null) return null;
                        if (_botClient != null)
                        {
                            var v1 = InputOnlineFile(message);
                            if (v1 != null)
                            {
                                var m1 = await _botClient.SendVideoAsync(chatIdToSend, v1,
                                    messageThreadId, message.Video.Duration, message.Video.Width, message.Video.Height,
                                    null,
                                    message.Caption,
                                    ParseMode.Html, replyToMessageId: messageIdToReplyToLong);
                                return new MessageSentResult(true, m1, m1.Chat.Type);
                            }
                        }

                        break;
                    }
                    case BotTypeApi.USER_BOT:
                        break;

                    case BotTypeApi.DISGUISED_BOT:
                        break;
                }

                break;
            }
            case MessageType.Voice:
                break;

            case MessageType.Document:
            {
                switch (_isbot)
                {
                    case BotTypeApi.REAL_BOT:
                    {
                        if (_botClient != null)
                        {
                            var d1 = InputOnlineFile(message);
                            if (d1 != null)
                            {
                                var m1 = await _botClient.SendDocumentAsync(
                                    chatIdToSend, d1, messageThreadId,
                                    null,
                                    message.Caption,
                                    ParseMode.Html, replyToMessageId: messageIdToReplyToLong);
                                return new MessageSentResult(true, m1, m1.Chat.Type);
                            }
                        }

                        break;
                    }
                    case BotTypeApi.USER_BOT:
                        break;

                    case BotTypeApi.DISGUISED_BOT:
                        break;
                }

                break;
            }
            case MessageType.Sticker:
            {
                switch (_isbot)
                {
                    case BotTypeApi.REAL_BOT:
                    {
                        if (_botClient != null)
                        {
                            var s1 = InputOnlineFile(message);
                            if (s1 != null)
                            {
                                var m1 = await _botClient.SendStickerAsync(chatIdToSend, s1,
                                    replyToMessageId: messageIdToReplyToLong);
                                return new MessageSentResult(true, m1, m1.Chat.Type);
                            }
                        }

                        break;
                    }
                    case BotTypeApi.USER_BOT:
                        break;

                    case BotTypeApi.DISGUISED_BOT:
                        break;
                }

                break;
            }
            case MessageType.Location:
                break;

            case MessageType.Contact:
                break;

            case MessageType.Venue:
                break;

            case MessageType.Game:
                break;

            case MessageType.VideoNote:
                break;

            case MessageType.Invoice:
                break;

            case MessageType.SuccessfulPayment:
                break;

            case MessageType.WebsiteConnected:
                break;

            case MessageType.ChatMembersAdded:
                break;

            case MessageType.ChatMemberLeft:
                break;

            case MessageType.ChatTitleChanged:
                break;

            case MessageType.ChatPhotoChanged:
                break;

            case MessageType.MessagePinned:
                break;

            case MessageType.ChatPhotoDeleted:
                break;

            case MessageType.GroupCreated:
                break;

            case MessageType.SupergroupCreated:
                break;

            case MessageType.ChannelCreated:
                break;

            case MessageType.MigratedToSupergroup:
                break;

            case MessageType.MigratedFromGroup:
                break;
        }

        return null;
    }

    private static InputFile? InputOnlineFile(Message? message)
    {
        if (message == null) return null;
        switch (message.Type)
        {
            case MessageType.Unknown:
                break;

            case MessageType.Text:
                break;

            case MessageType.Photo:
            {
                if (message.Photo != null)
                {
                    var idMax = FindMax(message.Photo);
                    return message.Photo != null
                        ? idMax == null ? null : new InputFileId(message.Photo[idMax.Value].FileId)
                        : null;
                }

                break;
            }
            case MessageType.Audio:
                break;

            case MessageType.Video:
            {
                return message.Video != null ? new InputFileId(message.Video.FileId) : null;
            }
            case MessageType.Voice:
                break;

            case MessageType.Document:
            {
                return message.Document != null ? new InputFileId(message.Document.FileId) : null;
            }
            case MessageType.Sticker:
            {
                return message.Sticker != null ? new InputFileId(message.Sticker.FileId) : null;
            }
            case MessageType.Location:
                break;

            case MessageType.Contact:
                break;

            case MessageType.Venue:
                break;

            case MessageType.Game:
                break;

            case MessageType.VideoNote:
                break;

            case MessageType.Invoice:
                break;

            case MessageType.SuccessfulPayment:
                break;

            case MessageType.WebsiteConnected:
                break;

            case MessageType.ChatMembersAdded:
                break;

            case MessageType.ChatMemberLeft:
                break;

            case MessageType.ChatTitleChanged:
                break;

            case MessageType.ChatPhotoChanged:
                break;

            case MessageType.MessagePinned:
                break;

            case MessageType.ChatPhotoDeleted:
                break;

            case MessageType.GroupCreated:
                break;

            case MessageType.SupergroupCreated:
                break;

            case MessageType.ChannelCreated:
                break;

            case MessageType.MigratedToSupergroup:
                break;

            case MessageType.MigratedFromGroup:
                break;
        }

        return null;
    }

    private static long? FindMax(IReadOnlyList<PhotoSize>? photo)
    {
        if (photo == null || photo.Count == 0)
            return null;

        var maxValue = -1;
        var maxPos = -1;

        for (var i = 0; i < photo.Count; i++)
            if (photo[i].Width > maxValue)
            {
                maxValue = photo[i].Width;
                maxPos = i;
            }

        return maxPos;
    }

    internal bool SendFileAsync(MessageOptions messageOptions)
    {
        if (messageOptions.DocumentInput == null)
            return false;

        var textToSend = GetTextToSend(messageOptions.Lang, messageOptions.DocumentInput);
        IReplyMarkup? reply = null;
        if (messageOptions.ReplyMarkupObject != null) reply = messageOptions.ReplyMarkupObject.GetReplyMarkupBot();
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                var inputOnlineFile = messageOptions.DocumentInput.GetOnlineFile();
                var userId = messageOptions.ChatId;
                if (userId == null)
                    return false;

                switch (messageOptions.DocumentInput.TextAsCaption)
                {
                    case TextAsCaption.AS_CAPTION:
                    {
                        if (_botClient == null) return true;
                        if (inputOnlineFile == null) return true;


                        _ = _botClient.SendDocumentAsync(userId, inputOnlineFile, null, null,
                            textToSend, messageOptions.ParseMode, replyMarkup: reply).Result;

                        return true;
                    }

                    case TextAsCaption.BEFORE_FILE:
                    {
                        if (_botClient == null)
                            return true;

                        if (textToSend != null)
                            _ = _botClient
                                .SendTextMessageAsync(userId, textToSend, messageOptions.MessageThreadId,
                                    messageOptions.ParseMode, replyMarkup: reply).Result;


                        if (inputOnlineFile != null)
                            _ = _botClient.SendDocumentAsync(userId, inputOnlineFile,
                                messageOptions.MessageThreadId,
                                parseMode: messageOptions.ParseMode).Result;


                        return true;
                    }

                    case TextAsCaption.AFTER_FILE:
                    {
                        if (_botClient == null) return true;
                        if (inputOnlineFile != null)
                            _ = _botClient.SendDocumentAsync(userId, inputOnlineFile,
                                    messageOptions.MessageThreadId,
                                    parseMode: messageOptions.ParseMode)
                                .Result;

                        if (textToSend != null)
                            _ = _botClient
                                .SendTextMessageAsync(userId, textToSend,
                                    messageOptions.MessageThreadId, messageOptions.ParseMode,
                                    replyMarkup: reply).Result;


                        return true;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(messageOptions.DocumentInput.TextAsCaption),
                            messageOptions.DocumentInput.TextAsCaption, null);
                }
            }

            case BotTypeApi.USER_BOT:
                switch (messageOptions.DocumentInput.TextAsCaption)
                {
                    case TextAsCaption.AS_CAPTION:
                    {
                        var tlFileToSend = messageOptions.DocumentInput.GetMediaTl(UserbotClient).Result;
                        if (tlFileToSend != null)
                        {
                            var r = tlFileToSend.SendMedia(messageOptions.Peer?.GetPeer(),
                                UserbotClient, textToSend, messageOptions.Username).Result;
                            return r != null;
                        }

                        break;
                    }

                    case TextAsCaption.BEFORE_FILE:
                    {
                        var r2 = SendMessage.SendMessageUserBot(UserbotClient, messageOptions.Peer?.GetPeer(),
                            new L(textToSend),
                            messageOptions.Username,
                            new TLReplyKeyboardHide(), messageOptions.Lang, messageOptions.ReplyToMessageId,
                            messageOptions.DisablePreviewLink).Result;
                        var tlFileToSend = messageOptions.DocumentInput.GetMediaTl(UserbotClient).Result;
                        if (tlFileToSend != null)
                        {
                            var r = tlFileToSend.SendMedia(messageOptions.Peer?.GetPeer(), UserbotClient, null,
                                messageOptions.Username, messageOptions.Lang).Result;
                            return r != null && r2 != null;
                        }

                        break;
                    }

                    case TextAsCaption.AFTER_FILE:
                    {
                        var tlFileToSend = messageOptions.DocumentInput.GetMediaTl(UserbotClient).Result;
                        if (tlFileToSend != null)
                        {
                            var r = tlFileToSend.SendMedia(messageOptions.Peer?.GetPeer(), UserbotClient,
                                null, messageOptions.Username, messageOptions.Lang).Result;
                            var r2 = SendMessage.SendMessageUserBot(UserbotClient, messageOptions.Peer?.GetPeer(),
                                new L(textToSend),
                                messageOptions.Username,
                                new TLReplyKeyboardHide(), messageOptions.Lang, messageOptions.ReplyToMessageId,
                                messageOptions.DisablePreviewLink).Result;
                            return r != null && r2 != null;
                        }

                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(messageOptions.DocumentInput.TextAsCaption),
                            messageOptions.DocumentInput.TextAsCaption, null);
                }

                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }


    private static string? GetTextToSend(string? lang, TelegramFile documentInput)
    {
        return documentInput.GetText(lang);
    }

    internal async Task<bool> UpdateUsername(string from, string to)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                break;

            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
            {
                TLChannel? c6 = null;
                if (UserbotClient != null)
                {
                    var c = await UserbotClient.ResolveUsernameAsync(from);
                    var c2 = c.Peer;
                    if (c2 == null)
                        return false;

                    var c5 = c.Chats[0];
                    if (c5 is not TLChannel) return false;
                    if (c5 is TLChannel channel)
                        c6 = channel;
                    if (c2 is not TLPeerChannel) return false;
                }

                try
                {
                    return c6 != null && UserbotClient != null &&
                           await UserbotClient.ChannelsUpdateUsername(c6.Id, c6.AccessHash, to);
                }
                catch (Exception? e2)
                {
                    Logger.WriteLine(e2);
                }

                return false;
            }
        }

        return false;
    }

    internal string? GetContactString()
    {
        return string.IsNullOrEmpty(_contactString)
            ? "You can find all our contact on our website https://polinetwork.org"
            : _contactString;
    }

    internal async Task<SuccessWithException?> IsAdminAsync(long userId, long chatId)
    {
        try
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    if (_botClient != null)
                    {
                        var admins = await _botClient.GetChatAdministratorsAsync(chatId);
                        var b1 = admins.Any(admin => admin.User.Id == userId);
                        return new SuccessWithException(b1);
                    }

                    break;
                case BotTypeApi.USER_BOT:
                    if (UserbotClient != null)
                    {
                        var r = await UserbotClient.ChannelsGetParticipant(
                            UserbotPeer.GetPeerChannelFromIdAndType(chatId, null),
                            UserbotPeer.GetPeerUserFromId(userId));

                        var b2 = r.Participant is TLChannelParticipantModerator or TLChannelParticipantCreator;
                        return new SuccessWithException(b2);
                    }

                    break;
                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new SuccessWithException(false, new NotImplementedException());
        }
        catch (Exception? e1)
        {
            return new SuccessWithException(false, e1);
        }
    }

    internal async Task<string?> ExportChatInviteLinkAsync(long chatId, long? accessHash)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                if (_botClient != null)
                    return await _botClient.ExportChatInviteLinkAsync(chatId);
                break;
            case BotTypeApi.USER_BOT:
                var channel = new TLChannel { AccessHash = accessHash, Id = (int)Convert.ToInt64(chatId) };
                if (UserbotClient == null)
                    return null;
                var invite = await UserbotClient.ChannelsGetInviteLink(channel);
                if (invite is TLChatInviteExported c1) return c1.Link;

                return null;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

#pragma warning disable CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono

    internal async Task<bool> SendMessageReactionAsync(int chatId, string emojiReaction, int messageId,
#pragma warning restore CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono
#pragma warning disable IDE0060 // Rimuovere il parametro inutilizzato
        ChatType chatType)
#pragma warning restore IDE0060 // Rimuovere il parametro inutilizzato
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                break;

            case BotTypeApi.USER_BOT:
                /*
                var updates =
                    await _userbotClient.SendMessageReactionAsync(
                        UserbotPeer.GetPeerFromIdAndType(chatId, chatType),
                        messageId, emojiReaction);
                return updates != null;
                */
                break;
            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    internal async Task<SuccessWithException?> BanUserFromGroup(long target, long groupChatId,
        string?[]? time, bool? revokeMessage)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:

                var untilDate = DateTimeClass.GetUntilDate(time);

                try
                {
                    if (untilDate == null)
                    {
                        if (_botClient != null)
                            await _botClient.BanChatMemberAsync(groupChatId, target, default, revokeMessage);

                        return new SuccessWithException(true);
                    }

                    if (_botClient != null)
                        await _botClient.BanChatMemberAsync(groupChatId, target, untilDate.Value, revokeMessage);
                    return new SuccessWithException(true);
                }
                catch (Exception? e1)
                {
                    return new SuccessWithException(false, e1);
                }

            case BotTypeApi.USER_BOT:
                return new SuccessWithException(false, new NotImplementedException());

            case BotTypeApi.DISGUISED_BOT:
                return new SuccessWithException(false, new NotImplementedException());

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal async Task<TLAbsDialogs?> GetLastDialogsAsync()
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                return null;

            case BotTypeApi.USER_BOT:
                if (UserbotClient != null) return await UserbotClient.GetUserDialogsAsync(limit: 100);
                break;
            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    internal async Task<ChatMember[]?> GetChatAdministratorsAsync(long? id)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                if (_botClient != null)
                    if (id != null)
                        return await _botClient.GetChatAdministratorsAsync(id);
                break;

            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    internal async Task<SuccessWithException?> UnBanUserFromGroup(long? target, long groupChatId)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                try
                {
                    if (target == null)
                        return new SuccessWithException(false, new ArgumentNullException());

                    if (_botClient != null) await _botClient.UnbanChatMemberAsync(groupChatId, target.Value, true);
                    return new SuccessWithException(true);
                }
                catch (Exception? e1)
                {
                    return new SuccessWithException(false, e1);
                }

            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new SuccessWithException(false, new NotImplementedException());
    }

    internal async Task<bool> LeaveChatAsync(long id)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                if (_botClient != null) await _botClient.LeaveChatAsync(id);
                return true;

            case BotTypeApi.USER_BOT:
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    public async Task<MessageSentResult?> SendPhotoAsync(long chatIdToSendTo, ObjectPhoto? objectPhoto,
        string? caption,
        ParseMode parseMode, ChatType chatTypeToSendTo, int? messageThreadId)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
            {
                if (_botClient != null)
                {
                    var m2 = objectPhoto?.GetTelegramBotInputOnlineFile();
                    if (m2 != null)
                    {
                        var m1 = await _botClient.SendPhotoAsync(
                            messageThreadId: messageThreadId,
                            chatId: chatIdToSendTo,
                            photo: m2,
                            caption: caption,
                            parseMode: parseMode);

                        return new MessageSentResult(true, m1, chatTypeToSendTo);
                    }
                }

                break;
            }

            case BotTypeApi.USER_BOT:

                if (objectPhoto != null)
                {
                    var photoFile = await objectPhoto.GetTelegramUserBotInputPhoto(UserbotClient);
                    if (photoFile?.Item1 == null)
                        return new MessageSentResult(false, null, chatTypeToSendTo);

                    if (UserbotClient != null)
                    {
                        var m2 = await UserbotClient.SendUploadedPhoto(
                            UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, chatTypeToSendTo), caption: caption,
                            file: photoFile.Item1);
                        return new MessageSentResult(m2 != null, m2, chatTypeToSendTo);
                    }
                }

                break;

            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new MessageSentResult(false, null, chatTypeToSendTo);
    }

    public async Task<long?> CreateGroup(string name, string? description, IEnumerable<long>? membersToInvite)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                return null;

            case BotTypeApi.USER_BOT:
            case BotTypeApi.DISGUISED_BOT:
                var users = new TLVector<TLAbsInputUser>();
                if (membersToInvite != null)
                    foreach (var userId in membersToInvite)
                        users.Add(new TLInputUser { UserId = (int)userId });

                try
                {
                    if (UserbotClient != null)
                    {
                        var r = await UserbotClient.Messages_CreateChat(name, users);
                        if (r is TLUpdates { Chats.Count: 1 } r2)
                        {
                            var c1 = r2.Chats[0];
                            if (c1 is TLChat c2)
                            {
                                //aggiorna la descrizione del gruppo appena creato
                                Logger.WriteLine(description);

                                try
                                {
                                    var tlChannel = new TLChannel { Id = c2.Id };
                                    var result = UserbotClient.Channels_EditDescription(tlChannel, description).Result;
                                    //todo: non mi convince, probabilmente non va
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }

                                return c2.Id;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteLine(e.Message);
                    Thread.Sleep(int.Parse(Regex.Match(e.Message, @"\d+").Value) * 1000);
                    return null;
                }

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public async Task<MessageSentResult?> SendVideoAsync(long chatIdToSendTo, ObjectVideo? video, string? caption,
        ParseMode parseMode, ChatType chatTypeToSendTo)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                try
                {
                    if (_botClient != null)
                    {
                        var v1 = video?.GetTelegramBotInputOnlineFile();
                        if (v1 != null)
                            if (video != null)
                            {
                                var m1 = await _botClient.SendVideoAsync(chatIdToSendTo, caption: caption,
                                    video: v1, duration: video.GetDuration(),
                                    height: video.GetHeight(),
                                    width: video.GetWidth(), parseMode: parseMode);
                                return new MessageSentResult(true, m1, chatTypeToSendTo);
                            }
                    }
                }
                catch
                {
                    return new MessageSentResult(false, null, chatTypeToSendTo);
                }

                break;

            case BotTypeApi.USER_BOT:
            {
                var videoFile = ObjectVideo.GetTelegramUserBotInputVideo(UserbotClient);
                if (videoFile == null)
                    return new MessageSentResult(false, null, chatTypeToSendTo);

                //UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, ChatType.Private), videoFile, caption
                var media2 = ObjectVideo.GetTLabsInputMedia();
                if (UserbotClient != null)
                {
                    var m2 = await UserbotClient.Messages_SendMedia(
                        UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, chatTypeToSendTo), media2);
                    return new MessageSentResult(m2 != null, m2, chatTypeToSendTo);
                }

                break;
            }
            case BotTypeApi.DISGUISED_BOT:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new MessageSentResult(false, null, chatTypeToSendTo);
    }

    internal async Task<bool> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBotAsync()
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                break;

            case BotTypeApi.USER_BOT:
            {
                return await UserBotFixBotAdmin.FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot2(this);
            }

            case BotTypeApi.DISGUISED_BOT:
                break;
        }

        return false;
    }

    public async Task AnswerCallbackQueryAsync(string callbackQueryId, string text)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                if (_botClient != null)
                    await _botClient.AnswerCallbackQueryAsync(callbackQueryId,
                        text);
                break;

            case BotTypeApi.USER_BOT:
            {
            }
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;
        }
    }

    public async Task EditMessageTextAsync(long chatId, int messageMessageId, string? text,
        ParseMode parseMode)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                if (_botClient != null)
                    if (text != null)
                        await _botClient.EditMessageTextAsync(chatId,
                            messageMessageId, text,
                            parseMode);
                break;

            case BotTypeApi.USER_BOT:
            {
            }
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;
        }
    }

    public async Task ForwardMessageAsync(ChatId chatId, ChatId idChatMessageFrom, int idChatMessageTo,
        bool? disableNotification, bool? protectContent, CancellationToken cancellationToken)
    {
        switch (_isbot)
        {
            case BotTypeApi.REAL_BOT:
                if (_botClient != null)

                    await _botClient.ForwardMessageAsync(chatId, idChatMessageFrom,
                        idChatMessageTo,
                        null,
                        disableNotification,
                        protectContent, cancellationToken);
                break;

            case BotTypeApi.USER_BOT:
            {
            }
                break;

            case BotTypeApi.DISGUISED_BOT:
                break;
        }
    }
}