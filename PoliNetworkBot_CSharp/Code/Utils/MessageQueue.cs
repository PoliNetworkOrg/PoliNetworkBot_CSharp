﻿#region

using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class MessageQueue
    {
        public readonly ChatType ChatType;
        public readonly string text;
        public KeyValuePair<long, TelegramBotAbstract> key;
        private ParseMode Parsemode;

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