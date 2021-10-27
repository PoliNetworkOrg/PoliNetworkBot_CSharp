using System;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class MessageSendScheduled : MessageSentResult
    {
        public Tuple<bool?, int, string> r1;
        public ScheduleMessageSentResult scheduleMessageSentResult;

        public MessageSendScheduled(ScheduleMessageSentResult scheduleMessageSentResult,
            object message, ChatType? chatType, Tuple<bool?, int, string> r1)
            : base(scheduleMessageSentResult == ScheduleMessageSentResult.SUCCESS, message, chatType)
        {
            this.scheduleMessageSentResult = scheduleMessageSentResult;
            this.r1 = r1;
        }
    }
}