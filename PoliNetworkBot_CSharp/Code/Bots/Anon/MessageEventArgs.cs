using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    [System.Serializable]
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