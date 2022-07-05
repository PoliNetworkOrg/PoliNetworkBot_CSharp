namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

public class MessageReply
{
    public int? MessageIdToReplyTo;
    private readonly ResultQueueEnum? _resultQueueEnum;

    public MessageReply(int? f1, ResultQueueEnum? f2)
    {
        MessageIdToReplyTo = f1;
        _resultQueueEnum = f2;
    }
}