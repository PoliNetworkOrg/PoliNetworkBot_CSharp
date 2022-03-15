using System;

namespace PoliNetworkBot_CSharp.Code.Enums
{
    /// <summary>
    /// Hides PENDING messages responding only ALLOWED, NOT_ALLOWED, NOT_DEFINED
    /// </summary>
    public class MessageAllowedStatus
    {
        private MessageAllowedStatusEnum _messageAllowedStatus;
        private DateTime? allowedTime;
        private readonly DateTime insertedTime = DateTime.Now;

        public MessageAllowedStatus(MessageAllowedStatusEnum allowedSpam, DateTime? allowedTime)
        {
            _messageAllowedStatus = allowedSpam;
            this.allowedTime = allowedTime;
        }
        
        /// <summary>
        /// Status of the Message
        /// </summary>
        /// <returns>ALLOWED, NOT_ALLOWED, NOT_DEFINED</returns>
        public MessageAllowedStatusEnum GetStatus()
        {
            switch(_messageAllowedStatus)
            {
                case MessageAllowedStatusEnum.ALLOWED:
                case MessageAllowedStatusEnum.PENDING:
                {
                    if (allowedTime == null || allowedTime > DateTime.Now || allowedTime.Value.AddHours(24) < DateTime.Now)
                        return MessageAllowedStatusEnum.NOT_DEFINED;
                    return MessageAllowedStatusEnum.ALLOWED;
                }
                case MessageAllowedStatusEnum.NOT_ALLOWED:
                case MessageAllowedStatusEnum.NOT_DEFINED:
                    return _messageAllowedStatus;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ForceAllowMessage()
        {
            allowedTime = DateTime.Now;
            _messageAllowedStatus = MessageAllowedStatusEnum.ALLOWED;
        }

        public void RemoveMessage(bool andFlagAsSpam)
        {
            _messageAllowedStatus = andFlagAsSpam ? MessageAllowedStatusEnum.NOT_ALLOWED : MessageAllowedStatusEnum.NOT_DEFINED;
        }

        public DateTime RemovalTime()
        {
            return allowedTime?.AddHours(24) ?? insertedTime.AddHours(24);
        }
    }
}