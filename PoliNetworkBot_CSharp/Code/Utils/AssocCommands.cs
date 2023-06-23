using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class AssocCommands
{
    public static void AssocWrite(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = await Assoc.Assoc_SendAsync(sender, e);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static void AssocWriteDry(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = await Assoc.Assoc_SendAsync(sender, e, true);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static void  AssocPublish(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = await Assoc.Assoc_Publish(sender, e);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static void AssocRead(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = await Assoc.Assoc_Read(sender, e, false);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static void AssocReadAll(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = await Assoc.Assoc_ReadAll(sender, e);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static void AssocDelete(ActionFuncGenericParams actionFuncGenericParams)
    {
        _ = await Assoc.Assoc_Delete(sender, e);
        return CommandExecutionState.SUCCESSFUL;
    }
}