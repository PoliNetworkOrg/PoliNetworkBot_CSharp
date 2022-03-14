using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils
{
    public class CallbackAssocVetoData : CallbackGenericData
    {
        public readonly string message;
        
        public CallbackAssocVetoData(List<CallbackOption> options, Action<CallbackGenericData> runAfterSelection, string message) : base(options, runAfterSelection)
        {
            this.message = message;
            this.options = options;
            RunAfterSelection = runAfterSelection;
        }
    }
}
