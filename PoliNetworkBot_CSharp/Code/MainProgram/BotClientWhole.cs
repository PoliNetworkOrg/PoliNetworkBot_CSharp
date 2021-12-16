using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using System;
using Telegram.Bot;

namespace PoliNetworkBot_CSharp.Code.MainProgram
{
    public class BotClientWhole
    {
        public TelegramBotClient botClient;
        public BotInfo bot;
        public Tuple<EventHandler<MessageEventArgs>, string> onmessageMethod2;

        public BotClientWhole(TelegramBotClient botClient, BotInfo bot)
        {
            this.botClient = botClient;
            this.bot = bot;
        }

        public BotClientWhole(TelegramBotClient botClient, BotInfo bot, Tuple<EventHandler<MessageEventArgs>, string> onmessageMethod2) : this(botClient, bot)
        {
            this.onmessageMethod2 = onmessageMethod2;
        }
    }
}