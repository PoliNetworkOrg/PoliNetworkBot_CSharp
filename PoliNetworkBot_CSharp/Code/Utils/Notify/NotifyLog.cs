using System;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Log;

namespace PoliNetworkBot_CSharp.Code.Utils.Notify;

public static class NotifyLog
{
    public static void SendInGroup(LogObject logObject, TelegramBotAbstract? telegramBotAbstract, string caption)
    {
        try
        {
            var x = logObject.ToTelegramFileContent(caption);
            if (telegramBotAbstract != null)
            {
                x?.SendToOwners(
                    telegramBotAbstract, null,
                    null, null,
                    FileTypeJsonEnum.STRING_JSONED,
                    new LogFileInfo()
                    {
                        filename = "notifyLog.json"
                    }
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}