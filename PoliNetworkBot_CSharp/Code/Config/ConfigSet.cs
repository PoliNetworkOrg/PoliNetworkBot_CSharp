using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Main;

namespace PoliNetworkBot_CSharp.Code.Config;

public static class ConfigSet
{
    public static CommandExecutionState WriteConfigToFile(MessageEventArgs? arg1, TelegramBotAbstract? arg2,
        string[]? arg3)
    {
        var c = ProgramUtil.BotConfigAll.BotInfos;
        if (c == null)
            return CommandExecutionState.NOT_TRIGGERED;

        NewConfig.WriteConfig(Paths.Info.ConfigBotsInfo, c);
        return CommandExecutionState.SUCCESSFUL;
    }
}