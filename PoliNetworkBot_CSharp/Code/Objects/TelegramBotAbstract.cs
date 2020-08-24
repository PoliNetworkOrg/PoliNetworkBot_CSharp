using PoliNetworkBot_CSharp.Bots.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

namespace PoliNetworkBot_CSharp
{
    public class TelegramBotAbstract
    {
        private readonly TelegramBotClient botClient;
        private readonly TelegramClient userbotClient;
        private readonly bool isbot;

        private readonly string website;
        private readonly string contactString;
        private readonly int id;

        public TelegramBotAbstract(TelegramBotClient bot_client, string website, string contactString)
        {
            this.botClient = bot_client;
            this.isbot = true;
            this.website = website;
            this.contactString = contactString;
        }

        public TelegramBotAbstract(TelegramClient userbot_client, string website, string contactString, long id)
        {
            this.userbotClient = userbot_client;
            this.isbot = false;
            this.website = website;
            this.contactString = contactString;
            this.id = (int)id;
        }

        internal string GetWebSite()
        {
            return this.website;
        }

        internal static TelegramBotAbstract GetFromRam(TelegramBotClient telegramBotClient_bot)
        {
            return Data.GlobalVariables.Bots[telegramBotClient_bot.BotId];
        }

        internal void DeleteMessageAsync(long chat_id, int messageId, ChatType chatType)
        {
            if (isbot)
            {
                this.botClient.DeleteMessageAsync(chat_id, messageId);
            }
            else
            {
                this.userbotClient.ChannelsDeleteMessageAsync(Utils.UserbotPeer.GetPeerChannelFromIdAndType(chat_id), new TLVector<int>() { messageId });
            }
        }

        internal async Task<int?> GetIDFromUsernameAsync(string target)
        {
            if (isbot)
            {
                //bot api does not allow that
                return null;
            }
            else
            {
                var r = await this.userbotClient.ResolveUsernameAsync(target);
                if (r.Peer != null)
                {
                    if (r.Peer is TLPeerUser tLPeerUser)
                    {
                        return tLPeerUser.UserId;
                    }
                }

                return null;
            }
        }

        internal void RestrictChatMemberAsync(long chat_id, int user_id, ChatPermissions permissions, DateTime untilDate)
        {
            if (isbot)
            {
                this.botClient.RestrictChatMemberAsync(chat_id, user_id, permissions, untilDate);
            }
        }

        internal int GetID()
        {
            if (isbot)
            {
                return this.botClient.BotId;
            }
            else
            {
                return this.id;
            }
        }

        internal bool SendTextMessageAsync(long chatid, string text,
            ChatType chatType, ParseMode parseMode = ParseMode.Default,
            bool force_reply = false, List<List<KeyboardButton>> reply_markup_keyboard = null)
        {
            if (isbot)
            {
                IReplyMarkup reply = null;
                if (force_reply && reply_markup_keyboard != null)
                {
                    reply = new ReplyKeyboardMarkup(reply_markup_keyboard);
                }
                else if (force_reply)
                {
                    reply = new ForceReplyMarkup();
                }

                this.botClient.SendTextMessageAsync(chatid, text, parseMode, replyMarkup: reply);
                return true;
            }
            else
            {
                var peer = Utils.UserbotPeer.GetPeerFromIdAndType(chatid, chatType);
                this.userbotClient.SendMessageAsync(peer, text);
                return true;
            }
        }

        internal async Task<bool> SendFileAsync(TelegramFile document_input, long chat_id, string text, TextAsCaption text_as_caption)
        {
            if (isbot)
            {
                InputOnlineFile inputOnlineFile = document_input.GetOnlineFile();
                switch (text_as_caption)
                {
                    case TextAsCaption.AS_CAPTION:
                        {
                            _ = await this.botClient.SendDocumentAsync(chat_id, inputOnlineFile, caption: text);
                            return true;
                        }

                    case TextAsCaption.BEFORE_FILE:
                        {
                            _ = await this.botClient.SendTextMessageAsync(chat_id, text);
                            _ = await this.botClient.SendDocumentAsync(chat_id, inputOnlineFile);
                            return true;
                        }

                    case TextAsCaption.AFTER_FILE:
                        {
                            _ = await this.botClient.SendDocumentAsync(chat_id, inputOnlineFile);
                            _ = await this.botClient.SendTextMessageAsync(chat_id, text);
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
            return this.contactString;
        }

        internal async Task<bool> IsAdminAsync(int user_id, long chat_id)
        {
            if (isbot)
            {
                ChatMember[] admins = await this.botClient.GetChatAdministratorsAsync(chat_id);
                foreach (var admin in admins)
                {
                    if (admin.User.Id == user_id)
                        return true;
                }
                return false;
            }
            else
            {
                TeleSharp.TL.Channels.TLChannelParticipant r = await this.userbotClient.ChannelsGetParticipant(
                    channel: Utils.UserbotPeer.GetPeerChannelFromIdAndType(chat_id),
                    user: Utils.UserbotPeer.GetPeerUserFromdId(user_id));

                if (r.Participant is TLChannelParticipantModerator || r.Participant is TLChannelParticipantCreator)
                {
                    return true;
                }

                return false;
            }
        }

        internal async Task<string> ExportChatInviteLinkAsync(long chat_id)
        {
            if (isbot)
            {
                return await this.botClient.ExportChatInviteLinkAsync(chat_id);
            }

            return null;
        }

        internal async Task<bool> SendMessageReactionAsync(int chatId, string emojiReaction, int messageId, Telegram.Bot.Types.Enums.ChatType chatType)
        {
            if (isbot)
            {
                //api does not allow that
                return false;
            }
            else
            {
                var updates = await this.userbotClient.SendMessageReactionAsync(Utils.UserbotPeer.GetPeerFromIdAndType(chatId, chatType), messageId, emojiReaction);
                if (updates == null)
                    return false;

                return true;
            }
        }

        internal bool BanUserFromGroup(int target, long group_chat_id, MessageEventArgs e, string[] time)
        {
            if (isbot)
            {
                DateTime? untilDate = Utils.DateTimeClass.GetUntilDate(time);

                try
                {
                    if (untilDate == null)
                    {
                        this.botClient.KickChatMemberAsync(group_chat_id, target);
                    }
                    else
                    {
                        this.botClient.KickChatMemberAsync(group_chat_id, target, untilDate: untilDate.Value);
                    }
                }
                catch
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        internal async Task<TLAbsDialogs> GetLastDialogsAsync()
        {
            if (isbot)
            {
                return null;
            }
            else
            {
                return await this.userbotClient.GetUserDialogsAsync(limit: 100);
            }
        }

        internal Task<Telegram.Bot.Types.ChatMember[]> GetChatAdministratorsAsync(long id)
        {
            if (isbot)
            {
                return this.botClient.GetChatAdministratorsAsync(id);
            }

            return null;
        }

        internal bool UnBanUserFromGroup(int target, long group_chat_id, MessageEventArgs e)
        {
            if (isbot)
            {
                try
                {
                    this.botClient.UnbanChatMemberAsync(group_chat_id, target);
                }
                catch
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        internal void LeaveChatAsync(long id)
        {
            if (isbot)
            {
                this.botClient.LeaveChatAsync(id);
            }
        }
    }
}