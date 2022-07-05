#region

using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

internal class MessageAnonToSendInQueue
{
    private readonly MessageEventArgs? _e;
    private readonly WebPost? _e2;

    public MessageAnonToSendInQueue(MessageEventArgs? e)
    {
        _e = e;
    }

    public MessageAnonToSendInQueue(WebPost? webPost)
    {
        _e2 = webPost;
    }

    internal string? GetUsername()
    {
        return _e?.Message?.From?.Username;
    }

    internal string? GetLanguageCode()
    {
        return _e?.Message?.From?.LanguageCode;
    }

    internal bool FromTelegram()
    {
        return _e != null;
    }

    internal Message? GetMessage()
    {
        return _e?.Message;
    }

    internal long? GetFromUserId()
    {
        return _e?.Message?.From?.Id;
    }

    internal long? GetFromUserIdOrPostId()
    {
        return _e != null ? _e.Message?.From?.Id : _e2?.postid;
    }

    internal async Task<MessageSentResult?> SendMessageInQueueAsync(TelegramBotAbstract? telegramBotAbstract)
    {
        if (telegramBotAbstract == null)
            return null;

        if (_e2 != null) return await SendMessageInQueue2Async(telegramBotAbstract);

        return null;
    }

    private async Task<MessageSentResult?> SendMessageInQueue2Async(TelegramBotAbstract? telegramBotAbstract)
    {
        var text = new Language(new Dictionary<string, string?>
        {
            { "en", _e2?.text }
        });
        if (telegramBotAbstract == null) return null;
        var m1 = await telegramBotAbstract.SendTextMessageAsync(ConfigAnon.ModAnonCheckGroup, text,
            ChatType.Group, "en", ParseMode.Html, null, null);
        return m1;
    }
}