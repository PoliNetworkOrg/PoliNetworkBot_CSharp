using System;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Log;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;

namespace PoliNetworkBot_CSharp.Code.Utils.Notify;

public static class NotifyLog
{
    public static void SendInGroup(LogObject logObject, TelegramBotAbstract? telegramBotAbstract, string caption)
    {
        try
        {
            var x = logObject.ToTelegramFileContent(caption);
            if (telegramBotAbstract != null)
                x?.SendToOwners3(
                    null,
                    telegramBotAbstract,
                    new LogFileInfo
                    {
                        filename = "notifyLog.json",
                        text = new L("it", "notifyLog")
                    },
                    null,
                    FileTypeJsonEnum.STRING_JSONED
                );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}