using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;

namespace PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;

public static class DatabaseClass
{
    public static async Task<CommandExecutionState> QueryBotExec(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        _ = await CommandDispatcher.QueryBot(true, e, sender);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> QueryBotSelect(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        _ = await CommandDispatcher.QueryBot(false, e, sender);
        return CommandExecutionState.SUCCESSFUL;
    }
}