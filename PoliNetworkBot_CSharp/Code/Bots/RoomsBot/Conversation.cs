using System;
using PoliNetworkBot_CSharp.Code.Utils.Logger;

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot;

/// <summary>
/// Class containing data and useful methods
/// related to a conversation between the bot and a user.
/// </summary>
public class Conversation
{
    private Data.Enums.ConversationState _state;

    public Data.Enums.ConversationState State
    {
        get => _state;
        set
        {
            Logger.WriteLine("changed state to => " + value);
            _state = value;
        }
    }

    public long ChatId { get; set; }
    public DateTime Date { get; set; }
    public Data.Enums.Function CurrentFunction { get; set; } = Data.Enums.Function.NULL_FUNCTION;
    public Data.Enums.Function CallbackNextFunction { get; set; } = Data.Enums.Function.NULL_FUNCTION;
    public string? Campus { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }

    public Conversation(long chatId)
    {
        ChatId = chatId;
        State = Data.Enums.ConversationState.START;
    }
}