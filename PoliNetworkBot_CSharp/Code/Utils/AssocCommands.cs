using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class AssocCommands
{
    public static void AssocWrite(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = Assoc.Assoc_SendAsync(actionFuncGenericParams.TelegramBotAbstract, actionFuncGenericParams.MessageEventArgs).Result;
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void AssocWriteDry(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = Assoc.Assoc_SendAsync(actionFuncGenericParams.TelegramBotAbstract, actionFuncGenericParams.MessageEventArgs, true).Result;
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void  AssocPublish(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = Assoc.Assoc_Publish(actionFuncGenericParams.TelegramBotAbstract, actionFuncGenericParams.MessageEventArgs).Result;
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void AssocRead(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = Assoc.Assoc_Read(actionFuncGenericParams.TelegramBotAbstract, actionFuncGenericParams.MessageEventArgs,false).Result;
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void AssocReadAll(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = Assoc.Assoc_ReadAll(actionFuncGenericParams.TelegramBotAbstract, actionFuncGenericParams.MessageEventArgs).Result;
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void AssocDelete(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = Assoc.Assoc_Delete(actionFuncGenericParams.TelegramBotAbstract, actionFuncGenericParams.MessageEventArgs).Result;
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }
}