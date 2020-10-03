using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class MessageSend
    {
        private readonly bool success;
        private readonly object message;
        private readonly ChatType chatType;

        public MessageSend(bool success, object message, ChatType chatType)
        {
            this.success = success;
            this.message = message;
            this.chatType = chatType;
        }

        internal object GetMessage()
        {
            return message;
        }

        internal ChatType GetChatType()
        {
            return this.chatType;
        }
    }
}