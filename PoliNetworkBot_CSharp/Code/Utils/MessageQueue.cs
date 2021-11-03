using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class MessageQueue
    {
        public KeyValuePair<long, TelegramBotAbstract> key;
        public Language text;
        public ChatType ChatType;
        public string Language;
        public ParseMode Parsemode;

        public string Username;


        public MessageQueue(KeyValuePair<long, TelegramBotAbstract> key, Language text, ChatType chatType, string language, ParseMode parsemode, string username)
        {
            this.key = key;
            this.text = text;
            this.ChatType = chatType;
            this.Language = language;
            Parsemode = parsemode;

            this.Username = username;

        }
    }
}