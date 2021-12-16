using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    public class MessageEventArgs
    {
        public Message Message;

        public MessageEventArgs(Message message)
        {
            Message = message;
        }
    }
}