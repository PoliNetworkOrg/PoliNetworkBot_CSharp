using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class MessageSendScheduled : MessageSend
    {
        public Code.Enums.ScheduleMessageSentResult scheduleMessageSentResult;
        public System.Tuple<bool?, int, string> r1;
        public MessageSendScheduled(Code.Enums.ScheduleMessageSentResult scheduleMessageSentResult, 
            object message, ChatType? chatType, System.Tuple<bool?, int, string> r1) 
            : base(scheduleMessageSentResult == Enums.ScheduleMessageSentResult.SUCCESS, message, chatType)
        {
            this.scheduleMessageSentResult = scheduleMessageSentResult;
            this.r1 = r1;
        }
    }
}