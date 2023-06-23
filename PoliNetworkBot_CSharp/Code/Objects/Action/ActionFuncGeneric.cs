using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Utils;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class ActionFuncGenericParams
{
    private MessageEventArgs? _messageEventArgs;
    private TelegramBotAbstract.TelegramBotAbstract? _telegramBotAbstract;
    private string[]? _strings;
    private Task<CommandExecutionState>? _taskCommandExecutionState;
    private Task? _task;

    public ActionFuncGenericParams Invoke()
    {
        if (_taskCommandExecutionState != null)
        {
            _taskCommandExecutionState.Start();
            _taskCommandExecutionState.Wait();
            this.CommandExecutionState = _taskCommandExecutionState.Result;
        }
        else if (this._task != null)
        {
            this._task.Start();
            this._task.Wait();
        }

        return this;
    }

    public CommandExecutionState CommandExecutionState { get; set; }
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


    public ActionFuncGenericParams? Invoke()
    {
        return this._action?.Invoke();
    }
}