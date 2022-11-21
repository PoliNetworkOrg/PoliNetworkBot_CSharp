#region

using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
public class MessageToDelete
{
    private readonly long? _accessHash;
    private readonly long _botId;
    private readonly long _chatId;
#pragma warning disable IDE0052 // Rimuovi i membri privati non letti
    private readonly ChatType? _chatType;
#pragma warning restore IDE0052 // Rimuovi i membri privati non letti
    private readonly int _messageId;
    private readonly DateTime _timeToDelete;

    public MessageToDelete(TLMessage r3, long chatId, DateTime timeToDelete, long botId, ChatType? chatType,
        long? accessHash)
    {
        _messageId = r3.Id;
        _chatId = chatId;
        _timeToDelete = timeToDelete;
        _botId = botId;
        _chatType = chatType;
        _accessHash = accessHash;
    }

    public MessageToDelete(Message r4, long chatId, DateTime timeToDelete, long botId, ChatType? chatType,
        long? accessHash)
    {
        _messageId = r4.MessageId;
        _chatId = chatId;
        _timeToDelete = timeToDelete;
        _botId = botId;
        _chatType = chatType;
        _accessHash = accessHash;
    }

    internal bool ToDelete()
    {
        return DateTime.Now > _timeToDelete;
    }

    internal async Task<bool> Delete(MessageEventArgs? e2)
    {
        if (GlobalVariables.Bots != null && GlobalVariables.Bots.ContainsKey(_botId) == false)
            return false;

        var bot = GlobalVariables.Bots?[_botId];
        if (bot == null)
            return false;

        try
        {
            return await bot.DeleteMessageAsync(_chatId, _messageId, _accessHash);
        }
        catch (Exception? e)
        {
            await NotifyUtil.NotifyOwnerWithLog2(e, bot, EventArgsContainer.Get(e2));
        }

        return false;
    }
}