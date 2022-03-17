#region

using Newtonsoft.Json;
using System;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class MessageEventArgs
    {
        public Message Message;

        public MessageEventArgs(Message message)
        {
            Message = message;
        }
    }
}