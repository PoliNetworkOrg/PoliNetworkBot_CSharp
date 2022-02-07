#region

using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using Telegram.Bot;

#endregion

namespace PoliNetworkBot_CSharp.Code.MainProgram
{
    public class BotClientWhole
    {
        public readonly Tuple<EventHandler<MessageEventArgs>, string> onmessageMethod2;
        public BotInfo bot;
        public TelegramBotClient botClient;
        public Dictionary<long, long> updatesMessageLastId;

        public BotClientWhole(TelegramBotClient botClient, BotInfo bot,
            Tuple<EventHandler<MessageEventArgs>, string> onmessageMethod2)
        {
            this.botClient = botClient;
            this.bot = bot;
            this.onmessageMethod2 = onmessageMethod2;
            updatesMessageLastId = new Dictionary<long, long>();
        }
    }
}