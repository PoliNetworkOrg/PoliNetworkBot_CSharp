using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Utils;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class ActionFuncGenericParams
{
    public MessageEventArgs? MessageEventArgs;
    public TelegramBotAbstract.TelegramBotAbstract? TelegramBotAbstract;
    public string[]? Strings;
    public Task<CommandExecutionState>? TaskCommandExecutionState;
    public Task? Task;

    public ActionFuncGenericParams Invoke()
    {
        if (Action != null)
        {
            this.Action.Invoke(this);
        }
        else if (TaskCommandExecutionState != null)
        {
            TaskCommandExecutionState.Start();
            TaskCommandExecutionState.Wait();
            this.CommandExecutionState = TaskCommandExecutionState.Result;
        }
        else if (this.Task != null)
        {
            this.Task.Start();
            this.Task.Wait();
        }

        return this;
    }

    public CommandExecutionState? CommandExecutionState;
    public Action<ActionFuncGenericParams>? Action;
}


[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class ActionFuncGeneric
{
    
    private readonly ActionFuncGenericParams? _action;
    

    public ActionFuncGeneric( ActionFuncGenericParams? action)
    {
        this._action = action;
    }

    public ActionFuncGeneric(Action<ActionFuncGenericParams>? action)
    {
        this._action = new ActionFuncGenericParams() { Action = action };
    }


    public ActionFuncGenericParams? Invoke()
    {
        return this._action?.Invoke();
    }
}