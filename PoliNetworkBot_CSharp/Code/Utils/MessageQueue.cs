#region

using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal class MessageQueue
{
    public readonly ChatType ChatType;
    private readonly ParseMode _parsemode;
    public readonly string Text;
    public KeyValuePair<long, TelegramBotAbstract?> Key;

    public MessageQueue(KeyValuePair<long, TelegramBotAbstract?> key, string text, ChatType chatType,
        ParseMode parsemode)
    {
        Key = key;
        Text = text;
        ChatType = chatType;
        _parsemode = parsemode;
    }
}