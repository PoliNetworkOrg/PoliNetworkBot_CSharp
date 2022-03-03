namespace PoliNetworkBot_CSharp.Code.Enums
{
    internal enum ScheduleMessageSentResult
    {
        NOT_THE_RIGHT_TIME,
        THE_MESSAGE_IS_NOT_SCHEDULED,
        FAILED_SEND,
        SUCCESS,
        WE_DONT_KNOW_IF_IT_HAS_BEEN_SENT,
        ALREADY_SENT
    }
}