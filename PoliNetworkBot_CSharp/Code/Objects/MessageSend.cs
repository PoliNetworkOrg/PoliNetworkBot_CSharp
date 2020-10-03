using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class MessageSend
    {
        private bool success;
        private object message;
        private ChatType chatType;

        public MessageSend(bool success, object message, ChatType chatType)
        {
            this.success = success;
            this.message = message;
            this.chatType = chatType;
        }

        internal object getMessage()
        {
            return message;
        }

        internal ChatType getChatType()
        {
            return this.chatType;
        }
    }
}