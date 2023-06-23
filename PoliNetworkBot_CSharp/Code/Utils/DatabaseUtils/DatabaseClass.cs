using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using SampleNuGet.Objects;
using SampleNuGet.Utils;

namespace PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;

public static class DatabaseClass
{
    public static void QueryBotExec(ActionFuncGenericParams actionFuncGenericParams)
    {
        var e = actionFuncGenericParams.MessageEventArgs;
        var sender = actionFuncGenericParams.TelegramBotAbstract;
        var queryBot = CommandDispatcher.QueryBot(true, e, sender);
        queryBot.Wait();
        actionFuncGenericParams.CommandExecutionState =  CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> QueryBotSelect(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        _ = await CommandDispatcher.QueryBot(false, e, sender);
        return CommandExecutionState.SUCCESSFUL;
    }
}