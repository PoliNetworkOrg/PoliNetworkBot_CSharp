using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp
{
    public class TelegramBotAbstract
    {
        private readonly TelegramBotClient botClient;
        private readonly bool isbot;
        private readonly string website;

        public TelegramBotAbstract(TelegramBotClient botClient, string website)
        {
            this.botClient = botClient;
            this.isbot = true;
            this.website = website;
        }

        internal string GetWebSite()
        {
            return this.website;
        }

        internal void DeleteMessageAsync(long id, int messageId)
        {
            if (isbot)
            {
                this.botClient.DeleteMessageAsync(id, messageId);
            }
        }

        internal void RestrictChatMemberAsync(long chat_id, int user_id, ChatPermissions permissions, DateTime untilDate)
        {
            if (isbot)
            {
                this.botClient.RestrictChatMemberAsync(chat_id, user_id, permissions, untilDate);
            }
        }

        internal void SendTextMessageAsync(long chatid, string text, Telegram.Bot.Types.Enums.ParseMode v = Telegram.Bot.Types.Enums.ParseMode.Default)
        {
            if (isbot)
            {
                this.botClient.SendTextMessageAsync(chatid, text, v);
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

        internal long? GetBotID()
        {
            if (isbot)
            {
                return this.botClient.BotId;
            }

            return null;
        }

        internal Task<Telegram.Bot.Types.ChatMember[]> GetChatAdministratorsAsync(long id)
        {
            if (isbot)
            {
                return this.botClient.GetChatAdministratorsAsync(id);
            }

            return null;
        }
    }
}