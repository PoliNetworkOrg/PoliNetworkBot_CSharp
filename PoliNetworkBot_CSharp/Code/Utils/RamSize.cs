#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class RamSize
{
    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public static async Task CheckRamSizeThread()
    {
        while (true)
        {
            try
            {
                var proc = Process.GetCurrentProcess();
                var ramUsed1 = proc.PrivateMemorySize64;
                var ramUsed2 = GC.GetTotalMemory(true);
                var message = "Ram size " + ramUsed1 + " " + ramUsed2;
                Logger.Logger.WriteLine(message);
                await SendMessage.SendMessageInAGroup(BotUtil.GetFirstModerationRealBot(), "en",
                    new Language(
                        new Dictionary<string, string?>
                        {
                            { "en", message }
                        }), null, Data.Constants.Groups.BackupGroup, ChatType.Group, ParseMode.Html, null, true);
            }
            catch (Exception? ex)
            {
                Logger.Logger.WriteLine(ex, LogSeverityLevel.ERROR);
            }

            Thread.Sleep(1000 * 60 * 60 * 12); // every 12 hours
        }
    }
}