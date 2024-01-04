using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;

namespace PoliNetworkBot_CSharp.Code.Utils.Ram;

public static class SendRamInfo
{
    public static CommandExecutionState GetRam(MessageEventArgs? arg1, TelegramBotAbstract? arg2, string[]? arg3)
    {
        try
        {
            var x = RamSize.SendFullRam(null, arg2);
            x.Wait();
            return CommandExecutionState.SUCCESSFUL;
        }
        catch
        {
            // ignored
        }

        return CommandExecutionState.ERROR_DEFAULT;
    }
}