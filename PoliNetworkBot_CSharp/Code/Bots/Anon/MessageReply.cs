namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

public class MessageReply
{
    private readonly ResultQueueEnum? _resultQueueEnum;
    public int? MessageIdToReplyTo;

    public MessageReply(int? f1, ResultQueueEnum? f2)
    {
        MessageIdToReplyTo = f1;
        _resultQueueEnum = f2;
    }
}