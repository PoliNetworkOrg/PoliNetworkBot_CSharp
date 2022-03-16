using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils
{
    public class CallbackAssocVetoData : CallbackGenericData
    {
        public string message;
        public bool Modified;
        public MessageEventArgs MessageEventArgs;
        
        public CallbackAssocVetoData(List<CallbackOption> options, Action<CallbackGenericData> runAfterSelection,
            string message, MessageEventArgs messageEventArgs) : base(options, runAfterSelection)
        {
            this.MessageEventArgs = messageEventArgs;
            Modified = false;
            this.message = message;
            this.options = options;
            RunAfterSelection = runAfterSelection;
        }

        public void OnCallback(string newMessage)
        {
            if (Modified) return;
            message = newMessage;
            Modified = true;
        }
    }
}
