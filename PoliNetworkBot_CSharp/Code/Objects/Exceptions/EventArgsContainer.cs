using System;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;

namespace PoliNetworkBot_CSharp.Code.Objects.Exceptions;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class EventArgsContainer
{
    public CallbackQueryEventArgs? CallbackQueryEventArgs;
    public MessageEventArgs? MessageEventArgs;
    public CallbackGenericData? CallbackGenericData;
 
    public static EventArgsContainer Get(MessageEventArgs? messageEventArgs)
    {
        return new EventArgsContainer() { MessageEventArgs = messageEventArgs };
    }

    public static EventArgsContainer Get(CallbackGenericData callbackGenericData)
    {
        return new EventArgsContainer() { CallbackGenericData = callbackGenericData };
    }


    public static EventArgsContainer Get(CallbackQueryEventArgs callbackQueryEventArgs)
    {
        return new EventArgsContainer() { CallbackQueryEventArgs = callbackQueryEventArgs };
    }

    public static EventArgsContainer? Get(EventArgsContainer? paramEvent)
    {
        return paramEvent;
    }
}