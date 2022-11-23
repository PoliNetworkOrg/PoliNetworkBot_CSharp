#region

using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal class MessageQueue
{
    private readonly ParseMode _parsemode;
    public readonly ChatType ChatType;
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