using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using Telegram.Bot.Types;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils
{
    [System.Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class CallbackAssocVetoData : CallbackGenericData
    {
        public readonly string message;
        public readonly MessageEventArgs MessageEventArgs;
        public readonly string MessageWithMetadata;
        public bool Modified;
        
        public CallbackAssocVetoData(List<CallbackOption> options, Action<CallbackGenericData> runAfterSelection,
            string message, MessageEventArgs messageEventArgs, string messageWithMetadata) : base(options, runAfterSelection)
        {
            MessageWithMetadata = messageWithMetadata;
            MessageEventArgs = messageEventArgs;
            this.message = message;
            this.options = options;
            RunAfterSelection = runAfterSelection;
        }

        public void OnCallback()
        {
            Modified = true;
        }
    }
}
