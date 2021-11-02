using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class LoggerItem
    {
        public KeyValuePair<long, TelegramBotAbstract> key;
        public Language text;
        public ChatType @private;
        public string v1;
        public ParseMode html;

        public string v2;


        public LoggerItem(KeyValuePair<long, TelegramBotAbstract> key, Language text, ChatType @private, string v1, ParseMode html, string v2)
        {
            this.key = key;
            this.text = text;
            this.@private = @private;
            this.v1 = v1;
            this.html = html;

            this.v2 = v2;

        }
    }
}