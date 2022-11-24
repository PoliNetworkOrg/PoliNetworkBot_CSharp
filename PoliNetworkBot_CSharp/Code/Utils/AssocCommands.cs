using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class AssocCommands
{
    public static async Task<CommandExecutionState> AssocWrite(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_SendAsync(sender, e);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> AssocWriteDry(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_SendAsync(sender, e, true);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> AssocPublish(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_Publish(sender, e);
        return CommandExecutionState.SUCCESSFUL;

    }

    public static async Task<CommandExecutionState> AssocRead(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_Read(sender, e, false);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> AssocReadAll(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_ReadAll(sender, e);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> AssocDelete(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_Delete(sender, e);
        return CommandExecutionState.SUCCESSFUL;
    }
}