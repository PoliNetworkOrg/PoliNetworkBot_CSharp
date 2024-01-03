using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;

public static class GithubToken
{
    public static CommandExecutionState Set(MessageEventArgs? arg1, TelegramBotAbstract? arg2, string[]? arg3)
    {
        if (arg2 == null)
            return CommandExecutionState.NOT_TRIGGERED;

        if (arg3 == null)
            return CommandExecutionState.NOT_TRIGGERED;

        if (arg3.Length == 0)
            return CommandExecutionState.NOT_TRIGGERED;


        var arg2GithubToken = arg3[0];

        arg2.GithubToken = arg2GithubToken == "null" ? null : arg2GithubToken;
        return CommandExecutionState.SUCCESSFUL;
    }
}