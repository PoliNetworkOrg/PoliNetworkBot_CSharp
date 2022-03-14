#region

using System;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class MessageSendScheduled : MessageSentResult
    {
        public readonly Tuple<bool?, int, string> R1;
        public readonly ScheduleMessageSentResult ScheduleMessageSentResult;

        public MessageSendScheduled(ScheduleMessageSentResult scheduleMessageSentResult,
            object message, ChatType? chatType, Tuple<bool?, int, string> r1)
            : base(scheduleMessageSentResult == ScheduleMessageSentResult.SUCCESS, message, chatType)
        {
            this.ScheduleMessageSentResult = scheduleMessageSentResult;
            this.R1 = r1;
        }
    }
}