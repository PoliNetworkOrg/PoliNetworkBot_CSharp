using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Utils;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class ActionFuncGenericParams
{
    public Action<ActionFuncGenericParams>? Action;

    public CommandExecutionState? CommandExecutionState;
    public MessageEventArgs? MessageEventArgs;
    public string[]? Strings;
    public Task? Task;
    public Task<CommandExecutionState>? TaskCommandExecutionState;
    public TelegramBotAbstract.TelegramBotAbstract? TelegramBotAbstract;

    public ActionFuncGenericParams Invoke()
    {
        if (Action != null)
        {
            Action.Invoke(this);
        }
        else if (TaskCommandExecutionState != null)
        {
            TaskCommandExecutionState.Start();
            TaskCommandExecutionState.Wait();
            CommandExecutionState = TaskCommandExecutionState.Result;
        }
        else if (Task != null)
        {
            Task.Start();
            Task.Wait();
        }

        return this;
    }
}

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class ActionFuncGeneric
{
    private readonly ActionFuncGenericParams? _action;


    public ActionFuncGeneric(ActionFuncGenericParams? action)
    {
        _action = action;
    }

    public ActionFuncGeneric(Action<ActionFuncGenericParams>? action)
    {
        _action = new ActionFuncGenericParams { Action = action };
    }


    public ActionFuncGenericParams? Invoke()
    {
        return _action?.Invoke();
    }
}