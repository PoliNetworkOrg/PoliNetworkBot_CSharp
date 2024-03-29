﻿using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;

namespace PoliNetworkBot_CSharp.Code.Utils.Running;

public static class RunningUtil
{
    public static Task<CommandExecutionState> KillYourself(MessageEventArgs? arg1, TelegramBotAbstract? arg2,
        string[]? arg3)
    {
        try
        {
            Environment.Exit(0);
        }
        catch
        {
            // ignored
        }

        return Task.FromResult(CommandExecutionState.SUCCESSFUL);
    }
}