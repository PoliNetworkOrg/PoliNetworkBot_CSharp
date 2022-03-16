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
        public readonly string message;
        public MessageEventArgs MessageEventArgs;
        public string messageWithMetadata;
        
        public CallbackAssocVetoData(List<CallbackOption> options, Action<CallbackGenericData> runAfterSelection,
            string message, MessageEventArgs messageEventArgs, string messageWithMetadata) : base(options, runAfterSelection)
        {
            this.messageWithMetadata = messageWithMetadata;
            MessageEventArgs = messageEventArgs;
            this.message = message;
            this.options = options;
            RunAfterSelection = runAfterSelection;
        }

        public void OnCallback(string newMessage)
        {
            messageWithMetadata = newMessage;
        }
    }
}
