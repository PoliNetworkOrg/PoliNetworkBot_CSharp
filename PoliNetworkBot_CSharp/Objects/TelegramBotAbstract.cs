using Telegram.Bot;

namespace PoliNetworkBot_CSharp
{
    public class TelegramBotAbstract
    {
        private readonly TelegramBotClient botClient;
        private readonly bool isbot;

        public TelegramBotAbstract(TelegramBotClient botClient)
        {
            this.botClient = botClient;
            this.isbot = true;
        }
    }
}