#region

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class MessageSentResult
{
    private readonly ChatType? _chatType;
    private readonly object? _message;
    private readonly bool _success;
    private long? _messageId;

    public MessageSentResult(bool success, object? message, ChatType? chatType)
    {
        _success = success;
        _message = message;
        _chatType = chatType;

        SetMessageId();
    }

    private void SetMessageId()
    {
        switch (_message)
        {
            case null:
                return;

            case TLMessage m1:
                _messageId = m1.Id;
                break;

            case Message m2:
                _messageId = m2.MessageId;
                break;
        }
    }

    internal object? GetMessage()
    {
        return _message;
    }

    internal ChatType? GetChatType()
    {
        return _chatType;
    }

    internal bool IsSuccess()
    {
        return _success;
    }

    internal long? GetMessageId()
    {
        return _messageId;
    }

    internal string GetLink(string chatId, bool isPrivate)
    {
        if (isPrivate)
            return "https://t.me/c/" + chatId + "/" + GetMessageId();

        return "https://t.me/" + chatId + "/" + GetMessageId();
    }
}