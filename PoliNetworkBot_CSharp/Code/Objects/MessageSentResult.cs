#region

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class MessageSentResult
    {
        private readonly ChatType? chatType;
        private readonly object message;
        private readonly bool success;
        private long? messageId;

        public MessageSentResult(bool success, object message, ChatType? chatType)
        {
            this.success = success;
            this.message = message;
            this.chatType = chatType;

            SetMessageId();
        }

        private void SetMessageId()
        {
            switch (message)
            {
                case null:
                    return;
                case TLMessage m1:
                    messageId = m1.Id;
                    break;
                case Message m2:
                    messageId = m2.MessageId;
                    break;
            }
        }

        internal object GetMessage()
        {
            return message;
        }

        internal ChatType? GetChatType()
        {
            return chatType;
        }

        internal bool IsSuccess()
        {
            return success;
        }

        internal long? GetMessageID()
        {
            return messageId;
        }

        internal string GetLink(string chatId, bool IsPrivate)
        {
            if (IsPrivate)
                return "https://t.me/c/" + chatId + "/" + GetMessageID();

            return "https://t.me/" + chatId + "/" + GetMessageID();
        }
    }
}