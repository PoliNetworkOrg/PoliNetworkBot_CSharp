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
        private const int VetoLowerBound = 9;
        private const int VetoHigherBound = 22;

        public MessageAllowedStatus(MessageAllowedStatusEnum allowedSpam, TimeSpan? timeSpan)
        {
            _messageAllowedStatus = allowedSpam;
            allowedTime = DateTime.Now;
            var now = DateTime.Now;
            var dayInCount = 0;
            while (!(timeSpan == null || timeSpan == TimeSpan.Zero))
            {
                if (now.Hour < VetoLowerBound) //only necessary in first iteration
                {
                    var diff = now - DateTime.Today.AddHours(VetoLowerBound);
                    allowedTime = allowedTime.Value.Add(diff);
                }

                var remainingTime = (DateTime.Today.AddDays(dayInCount).AddHours(VetoHigherBound) - allowedTime).Value.TotalMinutes > 0
                    ? DateTime.Today.AddDays(dayInCount).AddHours(VetoHigherBound) - allowedTime : TimeSpan.Zero;

                if (remainingTime >= timeSpan)
                {
                    allowedTime = allowedTime.Value.Add(timeSpan.Value);
                    timeSpan = TimeSpan.Zero;
                }
                else
                {
                    allowedTime = allowedTime.Value.Add(remainingTime.Value).Add(new TimeSpan((24 - VetoHigherBound) + VetoLowerBound, 0, 0));
                    timeSpan -= remainingTime;
                    dayInCount++;
                }
            }
        }

        /// <summary>
        /// Status of the Message
        /// </summary>
        /// <returns>ALLOWED, NOT_ALLOWED, NOT_DEFINED</returns>
        public MessageAllowedStatusEnum GetStatus()
        {
            switch (_messageAllowedStatus)
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