using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class MessageQueue
    {
        public KeyValuePair<long, TelegramBotAbstract> key;
        public string text;
        public ChatType ChatType;
        public ParseMode Parsemode;

        public MessageQueue(KeyValuePair<long, TelegramBotAbstract> key, string text, ChatType chatType, ParseMode parsemode)
        {
            this.key = key;
            this.text = text;
            this.ChatType = chatType;
            Parsemode = parsemode;
        }
    }
}