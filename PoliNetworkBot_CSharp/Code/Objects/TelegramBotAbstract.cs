#region

using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class TelegramBotAbstract
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _contactString;
        private readonly string mode;
        private string username = null;

        internal string GetMode()
        {
            return mode;
        }

        private readonly long _id;
        private readonly BotTypeApi _isbot;

        public readonly TelegramClient _userbotClient;

        private readonly string _website;

        internal async Task<string> GetBotUsernameAsync()
        {
            if (!string.IsNullOrEmpty(username))
                return username;

            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        User x = await this._botClient.GetMeAsync();
                        string u1 = x.Username;
                        if (u1.StartsWith("@"))
                            u1 = u1[1..];

                        username = u1;
                        return username;
                    }
                    break;

                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;
            }

            return null;
        }

        private TelegramBotAbstract(TelegramBotClient botClient, TelegramClient userBotClient, BotTypeApi botTypeApi, string website, string contactString, long id)
        {
            _userbotClient = userBotClient;
            _botClient = botClient;
            _isbot = botTypeApi;
            _website = website;
            _contactString = contactString;
            _id = id;
        }

        public TelegramBotAbstract(TelegramBotClient botClient, string website, string contactString,
            BotTypeApi botTypeApi, string mode) : this(botClient, null, botTypeApi, website, contactString, botClient.BotId)
        {
            this.mode = mode;
        }

        public TelegramBotAbstract(TelegramClient userbotClient, string website, string contactString, long id,
            BotTypeApi botTypeApi, string mode) : this(null, userbotClient, botTypeApi, website, contactString, id)
        {
            this.mode = mode;
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
            return GlobalVariables.Bots[telegramBotClientBot.BotId];
        }

        internal async Task<bool> DeleteMessageAsync(long chatId, int messageId, ChatType? chatType, long? accessHash)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        try
                        {
                            await _botClient.DeleteMessageAsync(chatId, messageId);
                        }
                        catch
                        {
                            return false;
                        }

                        return true;
                    }

                case BotTypeApi.USER_BOT:
                    {
                        TLAbsInputChannel peer = UserbotPeer.GetPeerChannelFromIdAndType(chatId, accessHash);

                        var r1 = await _userbotClient.ChannelsDeleteMessageAsync(peer,
                            new TLVector<int> { messageId });

                        return r1 != null;
                    }
                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        internal async Task<bool> PromoteChatMember(int userId, ChatId chatId)
        {
            switch (this._isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        try
                        {
                            await this._botClient.PromoteChatMemberAsync(chatId, userId, true, true, true, true, true, true, true, true);
                        }
                        catch (Exception e)
                        {
                            await Utils.NotifyUtil.NotifyOwners(e, this, 0);
                            return false;
                        }

                        return true;
                    }

                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;
            }

            return false;
        }

        internal async Task<Code.Objects.UserIdFound> GetIdFromUsernameAsync(string target)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        TelegramBotAbstract userBot = FindFirstUserBot();
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

        private TelegramBotAbstract FindFirstUserBot()
        {
            foreach (long b in Data.GlobalVariables.Bots.Keys)
            {
                var bot = Data.GlobalVariables.Bots[b];
                switch (bot._isbot)
                {
                    case BotTypeApi.REAL_BOT:
                        break;

                    case BotTypeApi.USER_BOT:
                        return bot;

                    case BotTypeApi.DISGUISED_BOT:
                        break;
                }
            }

            return null;
        }

        internal async Task<MessageSentResult> ForwardMessageAsync(int messageId, long idChatMessageFrom, long idChatMessageTo)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        var m = await _botClient.ForwardMessageAsync(idChatMessageTo, idChatMessageFrom, messageId);
                        return new MessageSentResult(true, m, m.Chat.Type);
                        break;
                    }
                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;
            }

            return null;
        }

        internal async Task RestrictChatMemberAsync(long chatId, int userId, ChatPermissions permissions,
            DateTime untilDate, ChatType chatType)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        switch (chatType)
                        {
                            case ChatType.Supergroup:
                                {
                                    await _botClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate);
                                    break;
                                }

                            case ChatType.Group:
                                {
                                    Console.WriteLine("Can't restrict a user in a group");
                                    break;
                                }
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

        internal async Task EditText(ChatId chatId, int messageId, string newText)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        await this._botClient.EditMessageTextAsync(chatId, messageId: messageId, text: newText);
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
                        await this._botClient.EditMessageReplyMarkupAsync(chatId: chatId, messageId: (int)messageId, replyMarkup: replyMarkup);
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

        internal long GetId()
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

        internal async Task<MessageSentResult> SendTextMessageAsync(long? chatid, Language text,
            ChatType? chatType, string lang, ParseMode parseMode,
            ReplyMarkupObject replyMarkupObject, string username, long? replyToMessageId = null, bool disablePreviewLink = false)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    IReplyMarkup reply = null;
                    if (replyMarkupObject != null) reply = replyMarkupObject.GetReplyMarkupBot();
                    var m2 = replyToMessageId == null ? 0 : replyToMessageId.Value;
                    Message m1 = await _botClient.SendTextMessageAsync(chatid, text.Select(lang), parseMode,
                        replyMarkup: reply, replyToMessageId: (int)m2, disableWebPagePreview: disablePreviewLink);
                    bool b1 = m1 != null;
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
                        bool b3 = m3 != null;
                        return new MessageSentResult(b3, m3, chatType);
                    }
                    catch (Exception e)
                    {
                        ;
                    }

                    return new MessageSentResult(false, null, chatType);

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new MessageSentResult(false, null, chatType);
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

                            case MessageType.Animation:
                                break;

                            case MessageType.Poll:
                                break;

                            case MessageType.Dice:
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

                        break;
                    }
                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        internal async Task<MessageSentResult> ForwardMessageAnonAsync(long chatIdToSend, Message message, Tuple<long?, Bots.Anon.ResultQueueEnum?> messageIdToReplyToLong)
        {
            int messageIdToReplyToInt = 0;
            if (messageIdToReplyToLong != null && messageIdToReplyToLong.Item1 != null)
            {
                messageIdToReplyToInt = (int)messageIdToReplyToLong.Item1.Value;
            }

            ;

            switch (message.Type)
            {
                case MessageType.Unknown:
                    break;

                case MessageType.Text:
                    {
                        switch (this._isbot)
                        {
                            case BotTypeApi.REAL_BOT:
                                {
                                    var m1 = await this._botClient.SendTextMessageAsync(chatIdToSend, message.Text,
                                        ParseMode.Html, replyToMessageId: messageIdToReplyToInt);
                                    return new MessageSentResult(m1 != null, m1, m1.Chat.Type);

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
                        switch (this._isbot)
                        {
                            case BotTypeApi.REAL_BOT:
                                {
                                    var m1 = await this._botClient.SendPhotoAsync(chatIdToSend, InputOnlineFile(message), message.Caption,
                                        ParseMode.Html, replyToMessageId: messageIdToReplyToInt);
                                    return new MessageSentResult(m1 != null, m1, m1.Chat.Type);
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
                        switch (this._isbot)
                        {
                            case BotTypeApi.REAL_BOT:
                                {
                                    var m1 = await this._botClient.SendVideoAsync(chatIdToSend, InputOnlineFile(message), message.Video.Duration, message.Video.Width, message.Video.Height, message.Caption, ParseMode.Html, replyToMessageId: messageIdToReplyToInt);
                                    return new MessageSentResult(m1 != null, m1, m1.Chat.Type);
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
                        switch (this._isbot)
                        {
                            case BotTypeApi.REAL_BOT:
                                {
                                    var m1 = await this._botClient.SendDocumentAsync(chatIdToSend, InputOnlineFile(message), message.Caption,
                                        ParseMode.Html, replyToMessageId: messageIdToReplyToInt);
                                    return new MessageSentResult(m1 != null, m1, m1.Chat.Type);
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
                        switch (this._isbot)
                        {
                            case BotTypeApi.REAL_BOT:
                                {
                                    var m1 = await this._botClient.SendStickerAsync(chatIdToSend, InputOnlineFile(message), replyToMessageId: messageIdToReplyToInt);
                                    return new MessageSentResult(m1 != null, m1, m1.Chat.Type);
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

                case MessageType.Animation:
                    break;

                case MessageType.Poll:
                    break;

                case MessageType.Dice:
                    break;
            }

            return null;
        }

        private Telegram.Bot.Types.InputFiles.InputOnlineFile InputOnlineFile(Message message)
        {
            switch (message.Type)
            {
                case MessageType.Unknown:
                    break;

                case MessageType.Text:
                    break;

                case MessageType.Photo:
                    {
                        int? idMax = FindMax(message.Photo);
                        if (idMax == null)
                            return null;

                        return new Telegram.Bot.Types.InputFiles.InputOnlineFile(message.Photo[idMax.Value].FileId);
                        break;
                    }
                case MessageType.Audio:
                    break;

                case MessageType.Video:
                    {
                        return new Telegram.Bot.Types.InputFiles.InputOnlineFile(message.Video.FileId);
                        break;
                    }
                case MessageType.Voice:
                    break;

                case MessageType.Document:
                    {
                        return new Telegram.Bot.Types.InputFiles.InputOnlineFile(message.Document.FileId);
                        break;
                    }
                case MessageType.Sticker:
                    {
                        return new Telegram.Bot.Types.InputFiles.InputOnlineFile(message.Sticker.FileId);
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

                case MessageType.Animation:
                    break;

                case MessageType.Poll:
                    break;

                case MessageType.Dice:
                    break;
            }

            return null;
        }

        private int? FindMax(PhotoSize[] photo)
        {
            if (photo == null || photo.Length == 0)
                return null;

            int maxValue = -1;
            int maxPos = -1;

            for (int i = 0; i < photo.Length; i++)
            {
                if (photo[i].Width > maxValue)
                {
                    maxValue = photo[i].Width;
                    maxPos = i;
                }
            }

            return maxPos;
        }

        internal async Task<bool> SendFileAsync(TelegramFile documentInput, Tuple<TLAbsInputPeer, long> peer,
            Language text,
            TextAsCaption textAsCaption, string username, string lang, long? replyToMessageId, bool disablePreviewLink)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    Telegram.Bot.Types.InputFiles.InputOnlineFile inputOnlineFile = documentInput.GetOnlineFile();
                    switch (textAsCaption)
                    {
                        case TextAsCaption.AS_CAPTION:
                            {
                                _ = await _botClient.SendDocumentAsync(peer.Item2, inputOnlineFile, text.Select(lang));
                                return true;
                            }

                        case TextAsCaption.BEFORE_FILE:
                            {
                                _ = await _botClient.SendTextMessageAsync(peer.Item2, text.Select(lang));
                                _ = await _botClient.SendDocumentAsync(peer.Item2, inputOnlineFile);
                                return true;
                            }

                        case TextAsCaption.AFTER_FILE:
                            {
                                _ = await _botClient.SendDocumentAsync(peer.Item2, inputOnlineFile);
                                _ = await _botClient.SendTextMessageAsync(peer.Item2, text.Select(lang));
                                return true;
                            }

                        default:
                            throw new ArgumentOutOfRangeException(nameof(textAsCaption), textAsCaption, null);
                    }

                    return false;

                case BotTypeApi.USER_BOT:
                    switch (textAsCaption)
                    {
                        case TextAsCaption.AS_CAPTION:
                            {
                                var tlFileToSend = await documentInput.GetMediaTl(_userbotClient);
                                var r = await tlFileToSend.SendMedia(peer.Item1, _userbotClient, text, username, lang);
                                return r != null;
                            }

                        case TextAsCaption.BEFORE_FILE:
                            {
                                var r2 = await SendMessage.SendMessageUserBot(_userbotClient, peer.Item1, text, username,
                                    new TLReplyKeyboardHide(), lang, replyToMessageId: replyToMessageId, disablePreviewLink: disablePreviewLink);
                                var tlFileToSend = await documentInput.GetMediaTl(_userbotClient);
                                var r = await tlFileToSend.SendMedia(peer.Item1, _userbotClient, null, username, lang);
                                return r != null && r2 != null;
                            }

                        case TextAsCaption.AFTER_FILE:
                            {
                                var tlFileToSend = await documentInput.GetMediaTl(_userbotClient);
                                var r = await tlFileToSend.SendMedia(peer.Item1, _userbotClient, null, username, lang);
                                var r2 = await SendMessage.SendMessageUserBot(_userbotClient, peer.Item1, text, username,
                                    new TLReplyKeyboardHide(), lang, replyToMessageId: replyToMessageId, disablePreviewLink: disablePreviewLink);
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
            switch (this._isbot)
            {
                case BotTypeApi.REAL_BOT:
                    break;

                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    {
                        var c = await this._userbotClient.ResolveUsernameAsync(from);
                        var c2 = c.Peer;
                        if (c2 == null)
                            return false;

                        var c5 = c.Chats[0];
                        if (c5 is TLChannel c6)
                        {
                            if (c2 is TLPeerChannel c3)
                            {
                                try
                                {
                                    return await this._userbotClient.ChannelsUpdateUsername(c6.Id, c6.AccessHash, to);
                                }
                                catch (Exception e2)
                                {
                                    ;
                                }
                            }
                        }

                        return false;
                    }
            }

            return false;
        }

        internal string GetContactString()
        {
            return _contactString;
        }

        internal async Task<Tuple<bool, Exception>> IsAdminAsync(int userId, long chatId)
        {
            try
            {
                switch (_isbot)
                {
                    case BotTypeApi.REAL_BOT:
                        var admins = await _botClient.GetChatAdministratorsAsync(chatId);
                        bool b1 = admins.Any(admin => admin.User.Id == userId);
                        return new Tuple<bool, Exception>(b1, null);
                        ;
                    case BotTypeApi.USER_BOT:
                        var r = await _userbotClient.ChannelsGetParticipant(
                            UserbotPeer.GetPeerChannelFromIdAndType(chatId, null),
                            UserbotPeer.GetPeerUserFromdId(userId));

                        bool b2 = r.Participant is TLChannelParticipantModerator ||
                               r.Participant is TLChannelParticipantCreator;
                        return new Tuple<bool, Exception>(b2, null);
                        ;
                    case BotTypeApi.DISGUISED_BOT:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return new Tuple<bool, Exception>(false, new NotImplementedException());
            }
            catch (Exception e1)
            {
                return new Tuple<bool, Exception>(false, e1);
            }
        }

        internal async Task<string> ExportChatInviteLinkAsync(long chatId)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return await _botClient.ExportChatInviteLinkAsync(chatId);
                    ;
                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        internal async Task<bool> SendMessageReactionAsync(int chatId, string emojiReaction, int messageId,
            ChatType chatType)
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

        internal async Task<Tuple<bool, Exception>> BanUserFromGroup(long target, long groupChatId, MessageEventArgs e, string[] time)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:

                    var untilDate = DateTimeClass.GetUntilDate(time);

                    try
                    {
                        if (untilDate == null)
                        {
                            await _botClient.KickChatMemberAsync(groupChatId, (int)target);
                            return new Tuple<bool, Exception>(true, null);
                        }

                        await _botClient.KickChatMemberAsync(groupChatId, (int)target, untilDate.Value);
                        return new Tuple<bool, Exception>(true, null);
                    }
                    catch (Exception e1)
                    {
                        return new Tuple<bool, Exception>(false, e1);
                    }

                    return new Tuple<bool, Exception>(true, null);
                    ;
                case BotTypeApi.USER_BOT:
                    return new Tuple<bool, Exception>(false, new NotImplementedException());

                case BotTypeApi.DISGUISED_BOT:
                    return new Tuple<bool, Exception>(false, new NotImplementedException());

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Tuple<bool, Exception>(false, new NotImplementedException());
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

        internal async Task<Tuple<bool, Exception>> UnBanUserFromGroup(int target, long groupChatId, MessageEventArgs e)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    try
                    {
                        await _botClient.UnbanChatMemberAsync(groupChatId, target);
                        return new Tuple<bool, Exception>(true, null);
                    }
                    catch (Exception e1)
                    {
                        return new Tuple<bool, Exception>(false, e1);
                    }

                    ;
                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Tuple<bool, Exception>(false, new NotImplementedException());
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

        public async Task<MessageSentResult> SendPhotoAsync(long chatIdToSendTo, ObjectPhoto objectPhoto, string caption,
            ParseMode parseMode, ChatType chatTypeToSendTo)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    {
                        Message m1 = await _botClient.SendPhotoAsync(chatIdToSendTo,
                            objectPhoto.GetTelegramBotInputOnlineFile(), caption, parseMode);

                        return new MessageSentResult(m1 != null, m1, chatTypeToSendTo);
                    }

                case BotTypeApi.USER_BOT:

                    var photoFile = await objectPhoto.GetTelegramUserBotInputPhoto(_userbotClient);
                    if (photoFile == null)
                        return new MessageSentResult(false, null, chatTypeToSendTo);

                    var m2 = await _userbotClient.SendUploadedPhoto(
                        UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, chatTypeToSendTo), caption: caption,
                        file: photoFile);
                    return new MessageSentResult(m2 != null, m2, chatTypeToSendTo);

                case BotTypeApi.DISGUISED_BOT:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new MessageSentResult(false, null, chatTypeToSendTo);
        }

        public async Task<bool> CreateGroup(string name, string description, IEnumerable<long> membersToInvite)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return false;

                case BotTypeApi.USER_BOT:
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    var users = new TLVector<TLAbsInputUser>();
                    foreach (var userId in membersToInvite) users.Add(new TLInputUser { UserId = (int)userId });

                    try
                    {
                        var r = await _userbotClient.Messages_CreateChat(name, users);
                        return r != null;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
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
                        var videoFile = await video.GetTelegramUserBotInputVideo(_userbotClient);
                        if (videoFile == null)
                            return new MessageSentResult(false, null, chatTypeToSendTo);

                        //UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, ChatType.Private), videoFile, caption
                        var media2 = video.GetTLabsInputMedia();
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
                        await Utils.UserBotFixBotAdmin.FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot2(this);
                    }
                    break;

                case BotTypeApi.DISGUISED_BOT:
                    break;
            }
        }
    }
}