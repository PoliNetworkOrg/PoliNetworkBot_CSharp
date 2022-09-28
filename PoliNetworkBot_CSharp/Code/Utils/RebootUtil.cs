using System.Linq;
using System.Management.Automation;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class RebootUtil
{
    private static void AnnounceReboot(TelegramBotAbstract? sender, MessageEventArgs messageEventArgs)
    {
        throw new System.NotImplementedException();
    }

    public static void RebootWithLog(TelegramBotAbstract? sender, MessageEventArgs e)
    {
      
        AnnounceReboot(sender, e);
    
        try
        {
            Logger.Logger.GetLog(sender, e);
        }
        catch
        {
            // ignored
        }

        Reboot();
        
    }
    
    private static void Reboot()
    {
        try
        {
            BackupUtil.BackupBeforeReboot();
        }
        catch
        {
            // ignored
        }

        using var powershell = PowerShell.Create();
        if (ScriptUtil.DoScript(powershell, "screen -ls", true).Aggregate("", (current, a) => current + a)
            .Contains("rebooter")) return;
        ScriptUtil.DoScript(powershell, "screen -d -m -S rebooter ./static/rebooter.sh", true);
    }
}