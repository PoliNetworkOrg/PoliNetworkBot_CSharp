using System;
using PoliNetworkBot_CSharp.Code.Objects.Action;

namespace PoliNetworkBot_CSharp.Code.Utils.Running;

public static class RunningUtil
{
    public static void KillYourself(ActionFuncGenericParams actionFuncGenericParams)
    {
        try
        {
            Environment.Exit(0);
        }
        catch
        {
            // ignored
        }

        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }
}