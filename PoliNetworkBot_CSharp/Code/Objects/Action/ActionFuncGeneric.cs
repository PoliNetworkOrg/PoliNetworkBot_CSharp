using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Utils;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class ActionFuncGeneric
{
    private readonly Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task<CommandExecutionState>>? _action;
    private readonly Func<MessageEventArgs, TelegramBotAbstract?, string[]?, CommandExecutionState>? _action2;
    private readonly Func<MessageEventArgs, TelegramBotAbstract?, CommandExecutionState>? _action3;
    private readonly Func<MessageEventArgs, TelegramBotAbstract?, Task<CommandExecutionState>>? _action4;

    public ActionFuncGeneric(Func<MessageEventArgs, TelegramBotAbstract?, string[]?, CommandExecutionState> action)
    {
        _action2 = action;
    }

    public ActionFuncGeneric(
        Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task<CommandExecutionState>> action)
    {
        _action = action;
    }

    public ActionFuncGeneric(Func<MessageEventArgs, TelegramBotAbstract?, CommandExecutionState> action)
    {
        _action3 = action;
    }

    public ActionFuncGeneric(Func<MessageEventArgs, TelegramBotAbstract?, Task<CommandExecutionState>> action)
    {
        _action4 = action;
    }

    public CommandExecutionState Invoke(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract, string[] args)
    {
        if (_action != null)
            return _action.Invoke(e, telegramBotAbstract, args).Result;
        if (_action2 != null)
            return _action2.Invoke(e, telegramBotAbstract, args);
        if (_action3 != null)
            return _action3.Invoke(e, telegramBotAbstract);
        if (_action4 != null)
            return _action4.Invoke(e, telegramBotAbstract).Result;

        return CommandExecutionState.NOT_TRIGGERED;
    }
}