using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Utils;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class ActionFuncGeneric
{
    private readonly Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string[]?,
        Task<CommandExecutionState>>? _action;

    private readonly Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string[]?, CommandExecutionState>?
        _action2;

    private readonly Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, CommandExecutionState>? _action3;

    private readonly Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, Task<CommandExecutionState>>?
        _action4;

    private readonly Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract, string[]?, Task>? _action5;
    private readonly Func<MessageEventArgs?, TelegramBotAbstract.TelegramBotAbstract?, Task>? _action6;

    public ActionFuncGeneric(
        Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string[]?, CommandExecutionState> action)
    {
        _action2 = action;
    }

    public ActionFuncGeneric(
        Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string[]?, Task<CommandExecutionState>> action)
    {
        _action = action;
    }

    public ActionFuncGeneric(
        Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, CommandExecutionState> action)
    {
        _action3 = action;
    }

    public ActionFuncGeneric(
        Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, Task<CommandExecutionState>> action)
    {
        _action4 = action;
    }

    public ActionFuncGeneric(Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract, string[]?, Task>? action)
    {
        _action5 = action;
    }

    public ActionFuncGeneric(Func<MessageEventArgs?, TelegramBotAbstract.TelegramBotAbstract?, Task>? action)
    {
        _action6 = action;
    }

    public CommandExecutionState Invoke(MessageEventArgs? e, TelegramBotAbstract.TelegramBotAbstract? telegramBotAbstract,
        string[]? args)
    {
        if (_action != null)
            if (e != null)
                return _action.Invoke(e, telegramBotAbstract, args).Result;
        if (_action2 != null)
            if (e != null)
                return _action2.Invoke(e, telegramBotAbstract, args);
        if (_action3 != null)
            if (e != null)
                return _action3.Invoke(e, telegramBotAbstract);
        if (_action4 != null)
            if (e != null)
                return _action4.Invoke(e, telegramBotAbstract).Result;
        if (_action5 != null)
        {
            if (e != null)
                if (telegramBotAbstract != null)
                    _action5.Invoke(e, telegramBotAbstract, args).Wait();
            return CommandExecutionState.SUCCESSFUL;
        }

        if (_action6 != null)
        {
            _action6.Invoke(e, telegramBotAbstract).Wait();
            return CommandExecutionState.SUCCESSFUL;
        }

        return CommandExecutionState.NOT_TRIGGERED;
    }
}