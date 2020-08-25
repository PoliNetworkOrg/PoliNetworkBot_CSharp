#region

using System;
using System.Collections.Generic;
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
        private readonly TelegramBotClient botClient;
        private readonly string contactString;
        private readonly int id;
        private readonly bool isbot;
        private readonly TelegramClient userbotClient;

        private readonly string website;

        public TelegramBotAbstract(TelegramBotClient bot_client, string website, string contactString)
        {
            botClient = bot_client;
            isbot = true;
            this.website = website;
            this.contactString = contactString;
        }

        public TelegramBotAbstract(TelegramClient userbot_client, string website, string contactString, long id)
        {
            userbotClient = userbot_client;
            isbot = false;
            this.website = website;
            this.contactString = contactString;
            this.id = (int) id;
        }

        internal string GetWebSite()
        {
            return website;
        }

        internal static TelegramBotAbstract GetFromRam(TelegramBotClient telegramBotClient_bot)
        {
            return GlobalVariables.Bots[telegramBotClient_bot.BotId];
        }

        internal void DeleteMessageAsync(long chat_id, int messageId, ChatType chatType)
        {
            if (isbot)
                botClient.DeleteMessageAsync(chat_id, messageId);
            else
                userbotClient.ChannelsDeleteMessageAsync(UserbotPeer.GetPeerChannelFromIdAndType(chat_id),
                    new TLVector<int> {messageId});
        }

        internal async Task<int?> GetIDFromUsernameAsync(string target)
        {
            if (isbot)
            {
                //bot api does not allow that
                return null;
            }

            var r = await userbotClient.ResolveUsernameAsync(target);
            if (r.Peer != null)
                if (r.Peer is TLPeerUser tLPeerUser)
                    return tLPeerUser.UserId;

            return null;
        }

        internal void RestrictChatMemberAsync(long chat_id, int user_id, ChatPermissions permissions,
            DateTime untilDate)
        {
            if (isbot) botClient.RestrictChatMemberAsync(chat_id, user_id, permissions, untilDate);
        }

        internal int GetID()
        {
            if (isbot)
                return botClient.BotId;
            return id;
        }

        internal bool SendTextMessageAsync(long chatid, string text,
            ChatType chatType, ParseMode parseMode = ParseMode.Default,
            bool force_reply = false, List<List<KeyboardButton>> reply_markup_keyboard = null)
        {
            if (isbot)
            {
                IReplyMarkup reply = null;
                if (force_reply && reply_markup_keyboard != null)
                    reply = new ReplyKeyboardMarkup(reply_markup_keyboard);
                else if (force_reply) reply = new ForceReplyMarkup();

                botClient.SendTextMessageAsync(chatid, text, parseMode, replyMarkup: reply);
                return true;
            }

            var peer = UserbotPeer.GetPeerFromIdAndType(chatid, chatType);
            userbotClient.SendMessageAsync(peer, text);
            return true;
        }

        internal async Task<bool> SendFileAsync(TelegramFile document_input, long chat_id, string text,
            TextAsCaption text_as_caption)
        {
            if (isbot)
            {
                var inputOnlineFile = document_input.GetOnlineFile();
                switch (text_as_caption)
                {
                    case TextAsCaption.AS_CAPTION:
                    {
                        _ = await botClient.SendDocumentAsync(chat_id, inputOnlineFile, text);
                        return true;
                    }

                    case TextAsCaption.BEFORE_FILE:
                    {
                        _ = await botClient.SendTextMessageAsync(chat_id, text);
                        _ = await botClient.SendDocumentAsync(chat_id, inputOnlineFile);
                        return true;
                    }

                    case TextAsCaption.AFTER_FILE:
                    {
                        _ = await botClient.SendDocumentAsync(chat_id, inputOnlineFile);
                        _ = await botClient.SendTextMessageAsync(chat_id, text);
                        return true;
                    }
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
            return contactString;
        }

        internal async Task<bool> IsAdminAsync(int user_id, long chat_id)
        {
            if (isbot)
            {
                var admins = await botClient.GetChatAdministratorsAsync(chat_id);
                foreach (var admin in admins)
                    if (admin.User.Id == user_id)
                        return true;
                return false;
            }

            var r = await userbotClient.ChannelsGetParticipant(
                UserbotPeer.GetPeerChannelFromIdAndType(chat_id),
                UserbotPeer.GetPeerUserFromdId(user_id));

            if (r.Participant is TLChannelParticipantModerator ||
                r.Participant is TLChannelParticipantCreator) return true;

            return false;
        }

        internal async Task<string> ExportChatInviteLinkAsync(long chat_id)
        {
            if (isbot) return await botClient.ExportChatInviteLinkAsync(chat_id);

            return null;
        }

        internal async Task<bool> SendMessageReactionAsync(int chatId, string emojiReaction, int messageId,
            ChatType chatType)
        {
            if (isbot)
            {
                //api does not allow that
                return false;
            }

            var updates =
                await userbotClient.SendMessageReactionAsync(UserbotPeer.GetPeerFromIdAndType(chatId, chatType),
                    messageId, emojiReaction);
            if (updates == null)
                return false;

            return true;
        }

        internal bool BanUserFromGroup(int target, long group_chat_id, MessageEventArgs e, string[] time)
        {
            if (isbot)
            {
                var untilDate = DateTimeClass.GetUntilDate(time);

                try
                {
                    if (untilDate == null)
                        botClient.KickChatMemberAsync(group_chat_id, target);
                    else
                        botClient.KickChatMemberAsync(group_chat_id, target, untilDate.Value);
                }
                catch
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        internal async Task<TLAbsDialogs> GetLastDialogsAsync()
        {
            if (isbot)
                return null;
            return await userbotClient.GetUserDialogsAsync(limit: 100);
        }

        internal Task<ChatMember[]> GetChatAdministratorsAsync(long id)
        {
            if (isbot) return botClient.GetChatAdministratorsAsync(id);

            return null;
        }

        internal bool UnBanUserFromGroup(int target, long group_chat_id, MessageEventArgs e)
        {
            if (isbot)
            {
                try
                {
                    botClient.UnbanChatMemberAsync(group_chat_id, target);
                }
                catch
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        internal void LeaveChatAsync(long id)
        {
            if (isbot) botClient.LeaveChatAsync(id);
        }
    }
}