#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;
using File = Telegram.Bot.Types.File;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class TelegramBotAbstract
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _contactString;

        private readonly long? _id;
        private readonly BotTypeApi _isbot;

        public readonly TelegramClient _userbotClient;

        private readonly string _website;
        private readonly string mode;
        private string username;

        private TelegramBotAbstract(TelegramBotClient botClient, TelegramClient userBotClient, BotTypeApi botTypeApi,
            string website, string contactString, long? id)
        {
            _userbotClient = userBotClient;
            _botClient = botClient;
            _isbot = botTypeApi;
            _website = website;
            _contactString = contactString;
            _id = id;
        }

        public TelegramBotAbstract(TelegramBotClient botClient, string website, string contactString,
            BotTypeApi botTypeApi, string mode) : this(botClient, null, botTypeApi, website, contactString,
            botClient.BotId)
        {
            this.mode = mode;
        }

        public TelegramBotAbstract(TelegramClient userbotClient, string website, string contactString, long id,
            BotTypeApi botTypeApi, string mode) : this(null, userbotClient, botTypeApi, website, contactString, id)
        {
            this.mode = mode;
        }

        internal async Task ExitGroupAsync(MessageEventArgs e)
        {
            try
            {
                switch (_isbot)
                {
                    case BotTypeApi.REAL_BOT:
                        {
                            await _botClient.LeaveChatAsync(e.Message.Chat.Id);
                        }
                        break;

                    case BotTypeApi.USER_BOT:
                        break;

                    case BotTypeApi.DISGUISED_BOT:
                        break;
                }
            }
            catch (Exception ex)
            {
                await NotifyUtil.NotifyOwners(ex, this, e);
            }
        }

        internal string GetMode()
        {
            return mode;
        }

        public async Task<TLAbsUpdates> AddUserIntoChannel(string userID, TLChannel channel)
        {
            if (string.IsNullOrEmpty(userID))
                return null;

            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    break;

                case BotTypeApi.USER_BOT:
                    {
                        try
                        {
                            var users = new TLVector<TLAbsInputUser>();
                            if (userID.StartsWith("@"))
                            {
                                var u = await UserbotPeer.GetPeerUserWithAccessHash(userID[1..], _userbotClient);
                                TLAbsInputUser input2 = new TLInputUser { AccessHash = u.AccessHash, UserId = u.UserId };
                                users.Add(input2);
                            }
                            else
                            {
                                users.Add(UserbotPeer.GetPeerUserFromdId(Convert.ToInt64(userID)));
                            }

                            var tLInputChannel = new TLInputChannel { ChannelId = channel.Id };
                            if (channel.AccessHash != null)
                                tLInputChannel.AccessHash = channel.AccessHash.Value;

                            var r = await _userbotClient.ChannelsInviteToChannel(tLInputChannel, users);
                            return r;
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

        internal async Task<TLChannelClass> UpgradeGroupIntoSupergroup(long? chatID)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    break;

                case BotTypeApi.USER_BOT:
                    {
                        var r = await _userbotClient.UpgradeGroupIntoSupergroup(chatID);
                        if (r is TLUpdates { Chats.Count: 2 } r2)
                        {
                            var c1 = r2.Chats[1];
                            if (c1 is TLChannel c2) return new TLChannelClass(c2);
                            ;
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
                        var r = await _userbotClient.Channels_EditDescription(channel, desc);
                        return r;
                    }

                case BotTypeApi.DISGUISED_BOT:
                    break;
            }

            return null;
        }

        internal async Task<string> GetBotUsernameAsync()
        {
            if (!string.IsNullOrEmpty(username))
                return username;

            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        var x = await _botClient.GetMeAsync();
                        var u1 = x.Username;
                        if (u1.StartsWith("@"))
                            u1 = u1[1..];

                        username = u1;
                        return username;
                    }

                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;
            }

            return null;
        }

        internal async Task<Tuple<Chat, Exception>> GetChat(long chatId)
        {
            Exception e = null;
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        try
                        {
                            return new Tuple<Chat, Exception>(await _botClient.GetChatAsync(chatId), e);
                        }
                        catch (Exception e2)
                        {
                            e = e2;
                        }

                        if (chatId <= 0) return new Tuple<Chat, Exception>(null, e);
                        await Task.Delay(100);

                        var chatidS = chatId.ToString();
                        chatidS = "-100" + chatidS;
                        var chatidSl = Convert.ToInt64(chatidS);
                        try
                        {
                            return new Tuple<Chat, Exception>(await _botClient.GetChatAsync(chatidSl), e);
                        }
                        catch (Exception e3)
                        {
                            return new Tuple<Chat, Exception>(null, e3);
                        }
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

        internal string GetWebSite()
        {
            return _website;
        }

        internal static TelegramBotAbstract GetFromRam(TelegramBotClient telegramBotClientBot)
        {
            return telegramBotClientBot.BotId == null ? null : GlobalVariables.Bots[telegramBotClientBot.BotId.Value];
        }

        internal async Task<bool> DeleteMessageAsync(long chatId, long messageId, long? accessHash)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        try
                        {
                            await _botClient.DeleteMessageAsync(chatId, (int)messageId);
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

                        var r1 = await _userbotClient.ChannelsDeleteMessageAsync(peer,
                            new TLVector<int> { (int)messageId });

                        return r1 != null;
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
                            await _botClient.PromoteChatMemberAsync(chatId, userIdInput.UserId, true, true, true, true,
                                true, true, true, true);
                        }
                        catch (Exception e)
                        {
                            await NotifyUtil.NotifyOwners(e, this, null);
                            return false;
                        }

                        return true;
                    }

                case BotTypeApi.USER_BOT:
                    {
                        try
                        {
                            TLAbsChannelParticipantRole role = new TLChannelRoleEditor();

                            await _userbotClient.ChannelsEditAdmin(
                                UserbotPeer.GetPeerChannelFromIdAndType(chatId.Identifier, accessHashChat),
                                userIdInput,
                                role);
                        }
                        catch (Exception e)
                        {
                            await NotifyUtil.NotifyOwners(e, this, null);
                            return false;
                        }

                        break;
                    }

                case BotTypeApi.DISGUISED_BOT:
                    break;
            }

            return false;
        }

        internal async Task<UserIdFound> GetIdFromUsernameAsync(string target)
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
                    var r = await _userbotClient.ResolveUsernameAsync(target);
                    return r.Peer switch
                    {
                        null => new UserIdFound(null, "UserbotCantFindTheIDofTarget(1)"),
                        TLPeerUser tLPeerUser => new UserIdFound(tLPeerUser.UserId, "UserbotCantFindTheIDofTarget(2)"),
                        _ => new UserIdFound(null, "UserbotCantFindTheIDofTarget(3)")
                    };

                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new UserIdFound(null, "BotIsNotOfAnyType");
        }

        private static TelegramBotAbstract FindFirstUserBot()
        {
            foreach (var bot in GlobalVariables.Bots.Keys.Select(b => GlobalVariables.Bots[b]))
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

        internal async Task<MessageSentResult> ForwardMessageAsync(int messageId, long idChatMessageFrom,
            long idChatMessageTo)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        var m = await _botClient.ForwardMessageAsync(idChatMessageTo, idChatMessageFrom, messageId);
                        return new MessageSentResult(true, m, m.Chat.Type);
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
                                        await _botClient.RestrictChatMemberAsync(chatId, userId.Value, permissions,
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

        internal async Task<Tuple<File, Stream>> DownloadFileAsync(Document d)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        var stream = new MemoryStream();
                        var f = await _botClient.GetInfoAndDownloadFileAsync(d.FileId, stream);

                        return new Tuple<File, Stream>(f, stream);
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

        internal async Task EditText(ChatId chatId, int messageId, string newText)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        await _botClient.EditMessageTextAsync(chatId, messageId, newText);
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

        internal async Task RemoveInlineKeyboardAsync(ChatId chatId, long messageId, InlineKeyboardMarkup replyMarkup)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
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
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return _botClient.BotId;

                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return _id;
        }

        /// <summary>
        /// Send text message
        /// </summary>
        /// <param name="chatid"></param>
        /// <param name="text"></param>
        /// <param name="chatType"></param>
        /// <param name="lang"></param>
        /// <param name="parseMode"></param>
        /// <param name="replyMarkupObject"></param>
        /// <param name="username"></param>
        /// <param name="replyToMessageId"></param>
        /// <param name="disablePreviewLink"></param>
        /// <param name="splitMessage"></param>
        /// <returns>MessageSentResult of the last message sent</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal async Task<MessageSentResult> SendTextMessageAsync(long? chatid, Language text,
            ChatType? chatType, string lang, ParseMode parseMode,
            ReplyMarkupObject replyMarkupObject, string username, long? replyToMessageId = null,
            bool disablePreviewLink = false, bool splitMessage = false)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    IReplyMarkup reply = null;
                    if (replyMarkupObject != null) reply = replyMarkupObject.GetReplyMarkupBot();
                    var m2 = replyToMessageId ?? 0;
                    var message = text.Select(lang);
                    Message m1;
                    while (splitMessage && message.Length > 4096)
                    {
                        m1 = await _botClient.SendTextMessageAsync(chatid, message[..4095], parseMode,
                            replyMarkup: reply, replyToMessageId: (int)m2, disableWebPagePreview: disablePreviewLink);
                        message = message[4095..];
                        Thread.Sleep(100);
                    }

                    m1 = await _botClient.SendTextMessageAsync(chatid, message, parseMode,
                        replyMarkup: reply, replyToMessageId: (int)m2, disableWebPagePreview: disablePreviewLink);
                    var b1 = m1 != null;
                    return new MessageSentResult(b1, m1, chatType);

                case BotTypeApi.USER_BOT:
                case BotTypeApi.DISGUISED_BOT:
                    if (chatid == null)
                        return null;
                    var peer = UserbotPeer.GetPeerFromIdAndType(chatid.Value, chatType);
                    try
                    {
                        TLAbsReplyMarkup replyMarkup = null;
                        if (replyMarkupObject != null) replyMarkup = replyMarkupObject.GetReplyMarkupUserBot();
                        var m3 = await SendMessage.SendMessageUserBot(_userbotClient,
                            peer, text, username, replyMarkup, lang, replyToMessageId, disablePreviewLink);
                        var b3 = m3 != null;
                        return new MessageSentResult(b3, m3, chatType);
                    }
                    catch (Exception e)
                    {
                        Logger.WriteLine(e);
                    }

                    return new MessageSentResult(false, null, chatType);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal async Task<bool> SendMedia(GenericFile genericFile, long chatid, ChatType chatType, string username,
            Language caption, string lang)
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
                        var media2 = await genericFile.GetMediaTl(_userbotClient);

                        var r = await media2.SendMedia(peer, _userbotClient, caption, username, lang);
                        return r != null;
                    }
                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        internal async Task<MessageSentResult> ForwardMessageAnonAsync(long chatIdToSend, Message message,
            int? messageIdToReplyToLong)
        {
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
                                    var m1 = await _botClient.SendTextMessageAsync(chatIdToSend, message.Text,
                                        ParseMode.Html, replyToMessageId: messageIdToReplyToLong);
                                    return new MessageSentResult(m1 != null, m1, m1.Chat.Type);
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
                                    var m1 = await _botClient.SendPhotoAsync(chatIdToSend, InputOnlineFile(message),
                                        message.Caption,
                                        ParseMode.Html, replyToMessageId: messageIdToReplyToLong);
                                    return new MessageSentResult(m1 != null, m1, m1.Chat.Type);
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
                                    var m1 = await _botClient.SendVideoAsync(chatIdToSend, InputOnlineFile(message),
                                        message.Video.Duration, message.Video.Width, message.Video.Height, null,
                                        message.Caption,
                                        ParseMode.Html, replyToMessageId: messageIdToReplyToLong);
                                    return new MessageSentResult(m1 != null, m1, m1.Chat.Type);
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
                                    var m1 = await _botClient.SendDocumentAsync(chatIdToSend, InputOnlineFile(message), null,
                                        message.Caption,
                                        ParseMode.Html, replyToMessageId: messageIdToReplyToLong);
                                    return new MessageSentResult(m1 != null, m1, m1.Chat.Type);
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
                                    var m1 = await _botClient.SendStickerAsync(chatIdToSend, InputOnlineFile(message),
                                        replyToMessageId: messageIdToReplyToLong);
                                    return new MessageSentResult(m1 != null, m1, m1.Chat.Type);
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

        private static InputOnlineFile InputOnlineFile(Message message)
        {
            switch (message.Type)
            {
                case MessageType.Unknown:
                    break;

                case MessageType.Text:
                    break;

                case MessageType.Photo:
                    {
                        var idMax = FindMax(message.Photo);
                        return idMax == null ? null : new InputOnlineFile(message.Photo[idMax.Value].FileId);
                    }
                case MessageType.Audio:
                    break;

                case MessageType.Video:
                    {
                        return new InputOnlineFile(message.Video.FileId);
                    }
                case MessageType.Voice:
                    break;

                case MessageType.Document:
                    {
                        return new InputOnlineFile(message.Document.FileId);
                    }
                case MessageType.Sticker:
                    {
                        return new InputOnlineFile(message.Sticker.FileId);
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

        private static long? FindMax(PhotoSize[] photo)
        {
            if (photo == null || photo.Length == 0)
                return null;

            var maxValue = -1;
            var maxPos = -1;

            for (var i = 0; i < photo.Length; i++)
                if (photo[i].Width > maxValue)
                {
                    maxValue = photo[i].Width;
                    maxPos = i;
                }

            return maxPos;
        }

        internal async Task<bool> SendFileAsync(TelegramFile documentInput, PeerAbstract peer,
            Language text,
            TextAsCaption textAsCaption, string username, string lang, long? replyToMessageId, bool disablePreviewLink)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        var inputOnlineFile = documentInput.GetOnlineFile();
                        switch (textAsCaption)
                        {
                            case TextAsCaption.AS_CAPTION:
                                {
                                    _ = await _botClient.SendDocumentAsync(peer.GetUserId(), inputOnlineFile,
                                        text.Select(lang));
                                    return true;
                                }

                            case TextAsCaption.BEFORE_FILE:
                                {
                                    _ = await _botClient.SendTextMessageAsync(peer.GetUserId(), text.Select(lang));
                                    _ = await _botClient.SendDocumentAsync(peer.GetUserId(), inputOnlineFile);
                                    return true;
                                }

                            case TextAsCaption.AFTER_FILE:
                                {
                                    _ = await _botClient.SendDocumentAsync(peer.GetUserId(), inputOnlineFile);
                                    _ = await _botClient.SendTextMessageAsync(peer.GetUserId(), text.Select(lang));
                                    return true;
                                }

                            default:
                                throw new ArgumentOutOfRangeException(nameof(textAsCaption), textAsCaption, null);
                        }
                    }

                case BotTypeApi.USER_BOT:
                    switch (textAsCaption)
                    {
                        case TextAsCaption.AS_CAPTION:
                            {
                                var tlFileToSend = await documentInput.GetMediaTl(_userbotClient);
                                var r = await tlFileToSend.SendMedia(peer.GetPeer(), _userbotClient, text, username, lang);
                                return r != null;
                            }

                        case TextAsCaption.BEFORE_FILE:
                            {
                                var r2 = await SendMessage.SendMessageUserBot(_userbotClient, peer.GetPeer(), text,
                                    username,
                                    new TLReplyKeyboardHide(), lang, replyToMessageId, disablePreviewLink);
                                var tlFileToSend = await documentInput.GetMediaTl(_userbotClient);
                                var r = await tlFileToSend.SendMedia(peer.GetPeer(), _userbotClient, null, username, lang);
                                return r != null && r2 != null;
                            }

                        case TextAsCaption.AFTER_FILE:
                            {
                                var tlFileToSend = await documentInput.GetMediaTl(_userbotClient);
                                var r = await tlFileToSend.SendMedia(peer.GetPeer(), _userbotClient, null, username, lang);
                                var r2 = await SendMessage.SendMessageUserBot(_userbotClient, peer.GetPeer(), text,
                                    username,
                                    new TLReplyKeyboardHide(), lang, replyToMessageId, disablePreviewLink);
                                return r != null && r2 != null;
                            }

                        default:
                            throw new ArgumentOutOfRangeException(nameof(textAsCaption), textAsCaption, null);
                    }

                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
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
                        var c = await _userbotClient.ResolveUsernameAsync(from);
                        var c2 = c.Peer;
                        if (c2 == null)
                            return false;

                        var c5 = c.Chats[0];
                        if (c5 is not TLChannel c6) return false;
                        if (c2 is not TLPeerChannel) return false;
                        try
                        {
                            return await _userbotClient.ChannelsUpdateUsername(c6.Id, c6.AccessHash, to);
                        }
                        catch (Exception e2)
                        {
                            Logger.WriteLine(e2);
                        }

                        return false;
                    }
            }

            return false;
        }

        internal string GetContactString()
        {
            return string.IsNullOrEmpty(_contactString)
                ? "You can find all our contact on our website https://polinetwork.org"
                : _contactString;
        }

        internal async Task<SuccessWithException> IsAdminAsync(long userId, long chatId)
        {
            try
            {
                switch (_isbot)
                {
                    case BotTypeApi.REAL_BOT:
                        var admins = await _botClient.GetChatAdministratorsAsync(chatId);
                        var b1 = admins.Any(admin => admin.User.Id == userId);
                        return new SuccessWithException(b1);
                        ;
                    case BotTypeApi.USER_BOT:
                        var r = await _userbotClient.ChannelsGetParticipant(
                            UserbotPeer.GetPeerChannelFromIdAndType(chatId, null),
                            UserbotPeer.GetPeerUserFromdId(userId));

                        var b2 = r.Participant is TLChannelParticipantModerator or TLChannelParticipantCreator;
                        return new SuccessWithException(b2);
                        ;
                    case BotTypeApi.DISGUISED_BOT:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return new SuccessWithException(false, new NotImplementedException());
            }
            catch (Exception e1)
            {
                return new SuccessWithException(false, e1);
            }
        }

        internal async Task<string> ExportChatInviteLinkAsync(long chatId, long? accessHash)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return await _botClient.ExportChatInviteLinkAsync(chatId);
                    ;
                case BotTypeApi.USER_BOT:
                    var channel = new TLChannel { AccessHash = accessHash, Id = (int)Convert.ToInt64(chatId) };
                    var invite = await _userbotClient.ChannelsGetInviteLink(channel);
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
                    ;
                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        internal async Task<SuccessWithException> BanUserFromGroup(long target, long groupChatId,
            string[] time, bool? revokeMessage)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:

                    var untilDate = DateTimeClass.GetUntilDate(time);

                    try
                    {
                        if (untilDate == null)
                        {
                            await _botClient.BanChatMemberAsync(groupChatId, target, default, revokeMessage);

                            return new SuccessWithException(true);
                        }

                        await _botClient.BanChatMemberAsync(groupChatId, target, untilDate.Value, revokeMessage);
                        return new SuccessWithException(true);
                    }
                    catch (Exception e1)
                    {
                        return new SuccessWithException(false, e1);
                    }

                    ;
                case BotTypeApi.USER_BOT:
                    return new SuccessWithException(false, new NotImplementedException());

                case BotTypeApi.DISGUISED_BOT:
                    return new SuccessWithException(false, new NotImplementedException());

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal async Task<TLAbsDialogs> GetLastDialogsAsync()
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return null;

                case BotTypeApi.USER_BOT:
                    return await _userbotClient.GetUserDialogsAsync(limit: 100);
                    ;
                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        internal async Task<ChatMember[]> GetChatAdministratorsAsync(long id)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return await _botClient.GetChatAdministratorsAsync(id);

                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        internal async Task<SuccessWithException> UnBanUserFromGroup(long target, long groupChatId)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    try
                    {
                        await _botClient.UnbanChatMemberAsync(groupChatId, target, true);
                        return new SuccessWithException(true);
                    }
                    catch (Exception e1)
                    {
                        return new SuccessWithException(false, e1);
                    }

                    ;
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
                    await _botClient.LeaveChatAsync(id);
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

        public async Task<MessageSentResult> SendPhotoAsync(long chatIdToSendTo, ObjectPhoto objectPhoto,
            string caption,
            ParseMode parseMode, ChatType chatTypeToSendTo)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        var m1 = await _botClient.SendPhotoAsync(chatIdToSendTo,
                            objectPhoto.GetTelegramBotInputOnlineFile(), caption, parseMode);

                        return new MessageSentResult(m1 != null, m1, chatTypeToSendTo);
                    }

                case BotTypeApi.USER_BOT:

                    var photoFile = await objectPhoto.GetTelegramUserBotInputPhoto(_userbotClient);
                    if (photoFile?.Item1 == null)
                        return new MessageSentResult(false, null, chatTypeToSendTo);

                    var m2 = await _userbotClient.SendUploadedPhoto(
                        UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, chatTypeToSendTo), caption: caption,
                        file: photoFile.Item1);
                    return new MessageSentResult(m2 != null, m2, chatTypeToSendTo);

                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new MessageSentResult(false, null, chatTypeToSendTo);
        }

        public async Task<long?> CreateGroup(string name, string description, IEnumerable<long> membersToInvite)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return null;

                case BotTypeApi.USER_BOT:
                case BotTypeApi.DISGUISED_BOT:
                    var users = new TLVector<TLAbsInputUser>();
                    foreach (var userId in membersToInvite) users.Add(new TLInputUser { UserId = (int)userId });

                    try
                    {
                        var r = await _userbotClient.Messages_CreateChat(name, users);
                        if (r is TLUpdates { Chats.Count: 1 } r2)
                        {
                            var c1 = r2.Chats[0];
                            if (c1 is TLChat c2)
                            {
                                //todo add description
                                Logger.WriteLine(description);

                                return c2.Id;
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

        public async Task<MessageSentResult> SendVideoAsync(long chatIdToSendTo, ObjectVideo video, string caption,
            ParseMode parseMode, ChatType chatTypeToSendTo)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    try
                    {
                        var m1 = await _botClient.SendVideoAsync(chatIdToSendTo, caption: caption,
                            video: video.GetTelegramBotInputOnlineFile(), duration: video.GetDuration(),
                            height: video.GetHeight(),
                            width: video.GetWidth(), parseMode: parseMode);
                        return new MessageSentResult(m1 != null, m1, chatTypeToSendTo);
                    }
                    catch
                    {
                        return new MessageSentResult(false, null, chatTypeToSendTo);
                    }

                case BotTypeApi.USER_BOT:
                    {
                        var videoFile = ObjectVideo.GetTelegramUserBotInputVideo(_userbotClient);
                        if (videoFile == null)
                            return new MessageSentResult(false, null, chatTypeToSendTo);

                        //UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, ChatType.Private), videoFile, caption
                        var media2 = ObjectVideo.GetTLabsInputMedia();
                        var m2 = await _userbotClient.Messages_SendMedia(
                            UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, chatTypeToSendTo), media2);
                        return new MessageSentResult(m2 != null, m2, chatTypeToSendTo);
                    }
                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new MessageSentResult(false, null, chatTypeToSendTo);
        }

        internal async Task FixTheFactThatSomeGroupsDoesNotHaveOurModerationBotAsync()
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    break;

                case BotTypeApi.USER_BOT:
                    {
                        await UserBotFixBotAdmin.FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot2(this);
                    }
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;
            }
        }

        public async Task AnswerCallbackQueryAsync(string callbackQueryId, string text)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
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

        public async Task EditMessageTextAsync(long chatId, int messageMessageId, string text,
            ParseMode parseMode)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
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
    }
}