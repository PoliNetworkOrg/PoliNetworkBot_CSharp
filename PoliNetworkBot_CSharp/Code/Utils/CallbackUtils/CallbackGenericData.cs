using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils
{
    public class CallbackGenericData
    {
        public string id;
        public List<CallbackOption> options = new();
        public Message message = null;
        internal TelegramBotAbstract Bot;
        internal MessageSentResult messageSent;
        internal DateTime? insertedTime;
        public Action<CallbackGenericData> RunAfterSelection;
        internal int selectedAnswer;
        internal CallbackQuery callBackQueryFromTelegram;

        public CallbackGenericData(List<CallbackOption> options, Action<CallbackGenericData> RunAfterSelection)
        {
            this.options = options;
            this.RunAfterSelection = RunAfterSelection;
        }

  
    }
}
