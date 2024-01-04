using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Objects.AbstractBot;

public class MessageOptions
{
    public TelegramFile? DocumentInput;
    public PeerAbstract? Peer;
    public long? ChatId { get; init; }
    public Language? Text { get; init; }
    public ChatType? ChatType { get; init; }
    public string? Lang { get; init; }
    public ParseMode? ParseMode { get; init; }
    public ReplyMarkupObject? ReplyMarkupObject { get; init; }
    public string? Username { get; init; }
    public long? ReplyToMessageId { get; init; }
    public bool DisablePreviewLink { get; init; }
    public bool SplitMessage { get; init; }
    public int? MessageThreadId { get; set; }
}