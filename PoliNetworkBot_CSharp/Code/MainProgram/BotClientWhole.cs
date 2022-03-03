#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using System;
using System.Collections.Generic;
using Telegram.Bot;

#endregion

namespace PoliNetworkBot_CSharp.Code.MainProgram
{
    public class BotClientWhole
    {
        public readonly Tuple<EventHandler<MessageEventArgs>, string> onmessageMethod2;
        public readonly TelegramBotClient botClient;
        public readonly BotInfoAbstract botInfoAbstract;
        public Dictionary<long, long> updatesMessageLastId;



        public BotClientWhole(TelegramBotClient botClient, BotInfoAbstract bot1, Tuple<EventHandler<MessageEventArgs>, string> onmessageMethod2)
        {
            this.botClient = botClient;
            this.botInfoAbstract = bot1;
            this.onmessageMethod2 = onmessageMethod2;
            updatesMessageLastId = new Dictionary<long, long>();
        }
    }
}