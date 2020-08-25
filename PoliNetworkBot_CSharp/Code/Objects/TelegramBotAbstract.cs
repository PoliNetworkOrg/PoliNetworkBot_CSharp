#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
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
        private readonly int _id;
        private readonly bool _isbot;
        private readonly TelegramClient _userbotClient;

        private readonly string _website;

        public TelegramBotAbstract(TelegramBotClient botClient, string website, string contactString)
        {
            _botClient = botClient;
            _isbot = true;
            _website = website;
            _contactString = contactString;
        }

        public TelegramBotAbstract(TelegramClient userbotClient, string website, string contactString, long id)
        {
            _userbotClient = userbotClient;
            _isbot = false;
            _website = website;
            _contactString = contactString;
            _id = (int) id;
        }

        internal string GetWebSite()
        {
            return _website;
        }

        internal static TelegramBotAbstract GetFromRam(TelegramBotClient telegramBotClientBot)
        {
            return GlobalVariables.Bots[telegramBotClientBot.BotId];
        }

        internal void DeleteMessageAsync(long chatId, int messageId, ChatType chatType)
        {
            if (_isbot)
                _botClient.DeleteMessageAsync(chatId, messageId);
            else
                _userbotClient.ChannelsDeleteMessageAsync(UserbotPeer.GetPeerChannelFromIdAndType(chatId),
                    new TLVector<int> {messageId});
        }

        internal async Task<int?> GetIdFromUsernameAsync(string target)
        {
            if (_isbot)
            {
                //bot api does not allow that
                return null;
            }

            var r = await _userbotClient.ResolveUsernameAsync(target);
            return r.Peer switch
            {
                null => null,
                TLPeerUser tLPeerUser => tLPeerUser.UserId,
                _ => null
            };
        }

        internal void RestrictChatMemberAsync(long chatId, int userId, ChatPermissions permissions,
            DateTime untilDate)
        {
            if (_isbot) _botClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate);
        }

        internal int GetId()
        {
            return _isbot ? _botClient.BotId : _id;
        }

        internal bool SendTextMessageAsync(long chatid, string text,
            ChatType chatType, ParseMode parseMode = ParseMode.Default,
            bool forceReply = false, List<List<KeyboardButton>> replyMarkupKeyboard = null)
        {
            if (_isbot)
            {
                IReplyMarkup reply = null;
                if (forceReply && replyMarkupKeyboard != null)
                    reply = new ReplyKeyboardMarkup(replyMarkupKeyboard);
                else if (forceReply) reply = new ForceReplyMarkup();

                _botClient.SendTextMessageAsync(chatid, text, parseMode, replyMarkup: reply);
                return true;
            }

            var peer = UserbotPeer.GetPeerFromIdAndType(chatid, chatType);
            _userbotClient.SendMessageAsync(peer, text);
            return true;
        }

        internal async Task<bool> SendFileAsync(TelegramFile documentInput, long chatId, string text,
            TextAsCaption textAsCaption)
        {
            if (_isbot)
            {
                var inputOnlineFile = documentInput.GetOnlineFile();
                switch (textAsCaption)
                {
                    case TextAsCaption.AS_CAPTION:
                    {
                        _ = await _botClient.SendDocumentAsync(chatId, inputOnlineFile, text);
                        return true;
                    }

                    case TextAsCaption.BEFORE_FILE:
                    {
                        _ = await _botClient.SendTextMessageAsync(chatId, text);
                        _ = await _botClient.SendDocumentAsync(chatId, inputOnlineFile);
                        return true;
                    }

                    case TextAsCaption.AFTER_FILE:
                    {
                        _ = await _botClient.SendDocumentAsync(chatId, inputOnlineFile);
                        _ = await _botClient.SendTextMessageAsync(chatId, text);
                        return true;
                    }
                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(textAsCaption), textAsCaption, null);
                }
            }
            else
            {
                return false;
            }

            return false;
        }

        internal string GetContactString()
        {
            return _contactString;
        }

        internal async Task<bool> IsAdminAsync(int userId, long chatId)
        {
            if (_isbot)
            {
                var admins = await _botClient.GetChatAdministratorsAsync(chatId);
                return admins.Any(admin => admin.User.Id == userId);
            }

            var r = await _userbotClient.ChannelsGetParticipant(
                UserbotPeer.GetPeerChannelFromIdAndType(chatId),
                UserbotPeer.GetPeerUserFromdId(userId));

            return r.Participant is TLChannelParticipantModerator ||
                   r.Participant is TLChannelParticipantCreator;
        }

        internal async Task<string> ExportChatInviteLinkAsync(long chatId)
        {
            if (_isbot) return await _botClient.ExportChatInviteLinkAsync(chatId);

            return null;
        }

        internal async Task<bool> SendMessageReactionAsync(int chatId, string emojiReaction, int messageId,
            ChatType chatType)
        {
            if (_isbot)
            {
                //api does not allow that
                return false;
            }

            var updates =
                await _userbotClient.SendMessageReactionAsync(UserbotPeer.GetPeerFromIdAndType(chatId, chatType),
                    messageId, emojiReaction);
            return updates != null;
        }

        internal bool BanUserFromGroup(int target, long groupChatId, MessageEventArgs e, string[] time)
        {
            if (!_isbot) return false;
            
            var untilDate = DateTimeClass.GetUntilDate(time);

            try
            {
                if (untilDate == null)
                    _botClient.KickChatMemberAsync(groupChatId, target);
                else
                    _botClient.KickChatMemberAsync(groupChatId, target, untilDate.Value);
            }
            catch
            {
                return false;
            }

            return true;

        }

        internal async Task<TLAbsDialogs> GetLastDialogsAsync()
        {
            if (_isbot)
                return null;
            return await _userbotClient.GetUserDialogsAsync(limit: 100);
        }

        internal Task<ChatMember[]> GetChatAdministratorsAsync(long id)
        {
            return _isbot ? _botClient.GetChatAdministratorsAsync(id) : null;
        }

        internal bool UnBanUserFromGroup(int target, long groupChatId, MessageEventArgs e)
        {
            if (!_isbot) 
                return false;
            
            try
            {
                _botClient.UnbanChatMemberAsync(groupChatId, target);
            }
            catch
            {
                return false;
            }

            return true;

        }

        internal void LeaveChatAsync(long id)
        {
            if (_isbot) _botClient.LeaveChatAsync(id);
        }
    }
}