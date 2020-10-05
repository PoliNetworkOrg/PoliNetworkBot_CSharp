using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class MessageSend
    {
        private readonly bool success;
        private readonly object message;
        private readonly ChatType? chatType;
        private int? messageId;

        public MessageSend(bool success, object message, ChatType? chatType)
        {
            this.success = success;
            this.message = message;
            this.chatType = chatType;

            SetMessageId();
        }

        private void SetMessageId()
        {
            if (this.message == null)
                return;

            if (this.message is TLMessage m1)
            {
                this.messageId = m1.Id;
            }

            if (this.message is Message m2)
            {
                this.messageId = m2.MessageId;
            }
        }

        internal object GetMessage()
        {
            return message;
        }

        internal ChatType? GetChatType()
        {
            return this.chatType;
        }

        internal bool IsSuccess()
        {
            return success;
        }

        internal long? GetMessageID()
        {
            return messageId;
      
        }
    }
}