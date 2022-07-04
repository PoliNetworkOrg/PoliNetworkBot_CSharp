#region

using System;
using Newtonsoft.Json;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MessageEventArgs
{
    public Message? Message;

    public MessageEventArgs(Message? message)
    {
        Message = message;
    }
}