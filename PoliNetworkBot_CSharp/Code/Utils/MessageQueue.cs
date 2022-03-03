#region

using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class MessageQueue
    {
        public readonly ChatType ChatType;
        public readonly string text;
        public KeyValuePair<long, TelegramBotAbstract> key;
        private readonly ParseMode Parsemode;

        public MessageQueue(KeyValuePair<long, TelegramBotAbstract> key, string text, ChatType chatType,
            ParseMode parsemode)
        {
            this.key = key;
            this.text = text;
            ChatType = chatType;
            Parsemode = parsemode;
        }
    }
}