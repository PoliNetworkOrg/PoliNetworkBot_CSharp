#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CallbackAssocVetoData : CallbackGenericData
{
    public readonly string message;
    public readonly MessageEventArgs MessageEventArgs;
    public readonly string MessageWithMetadata;
    public bool Modified;

    public CallbackAssocVetoData(List<CallbackOption> options, Action<CallbackGenericData> runAfterSelection,
        string message, MessageEventArgs messageEventArgs, string messageWithMetadata) : base(options,
        runAfterSelection)
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