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
        public readonly TelegramBotClient botClient;
        public readonly BotInfoAbstract botInfoAbstract;
        public readonly Tuple<EventHandler<MessageEventArgs>, string> onmessageMethod2;
        public Dictionary<long, long> updatesMessageLastId;


        public BotClientWhole(TelegramBotClient botClient, BotInfoAbstract bot1,
            Tuple<EventHandler<MessageEventArgs>, string> onmessageMethod2)
        {
            this.botClient = botClient;
            botInfoAbstract = bot1;
            this.onmessageMethod2 = onmessageMethod2;
            updatesMessageLastId = new Dictionary<long, long>();
        }
    }
}