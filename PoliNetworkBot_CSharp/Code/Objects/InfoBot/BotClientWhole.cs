#region

using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using Telegram.Bot;

#endregion

namespace PoliNetworkBot_CSharp.Code.MainProgram;

public class BotClientWhole
{
    public readonly TelegramBotClient? BotClient;
    public readonly BotInfoAbstract BotInfoAbstract;
    public readonly OnMessageMethodObject OnmessageMethod2;
    public readonly Dictionary<long, long> UpdatesMessageLastId;

    public BotClientWhole(TelegramBotClient? botClient, BotInfoAbstract bot1,
        OnMessageMethodObject onmessageMethod2)
    {
        BotClient = botClient;
        BotInfoAbstract = bot1;
        OnmessageMethod2 = onmessageMethod2;
        UpdatesMessageLastId = new Dictionary<long, long>();
    }
}