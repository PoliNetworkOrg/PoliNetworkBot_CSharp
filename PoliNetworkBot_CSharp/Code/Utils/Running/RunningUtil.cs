using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;

namespace PoliNetworkBot_CSharp.Code.Utils.Running;

public static class RunningUtil
{
    public static void  KillYourself(ActionFuncGenericParams actionFuncGenericParams)
    {
        try
        {
            Environment.Exit(0);
        }
        catch
        {
            ;
        }

        return Task.FromResult(CommandExecutionState.SUCCESSFUL);
    }
}