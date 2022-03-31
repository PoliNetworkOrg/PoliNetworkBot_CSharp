using System;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Data
{
    public class TelegramUser
    {
        public readonly string username;
        public readonly long? id;

        public TelegramUser(string v)
        {
            this.username = v;
        }

        public TelegramUser(long v)
        {
            this.id = v;
        }

        public TelegramUser(long v1, string v2)
        {
            this.id = v1;
            this.username = v2;
        }

        internal bool Matches(User from)
        {
            return from != null && Matches(from.Id, from.Username);
        }

        internal bool Matches(long userIdParam, string usernameParam)
        {
            return id switch
            {
                null => !string.IsNullOrEmpty(username) && usernameParam == username,
                _ => id == userIdParam,

            };
        }
    }
}