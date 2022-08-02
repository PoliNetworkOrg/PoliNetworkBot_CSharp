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
    private static RamUsed? _ramUsedStatic;

    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public static async Task CheckRamSizeThread()
    {
        while (true)
        {
            await CheckRamSizeThread2();
            Thread.Sleep(1000 * 60 * 60 * 12); // every 12 hours
        }
    }

    private static async Task CheckRamSizeThread2()
    {
        try
        {
            var proc = Process.GetCurrentProcess();
            var procPrivateMemorySize64 = proc.PrivateMemorySize64;
            var ramUsed2 = GC.GetTotalMemory(true);
            var ramUsed = new RamUsed(procPrivateMemorySize64, ramUsed2);
            var firstTime = _ramUsedStatic == null;
            _ramUsedStatic ??= ramUsed;

            if (!_ramUsedStatic.InferioreDi(ramUsed) && firstTime == false)
            {
                _ramUsedStatic = ramUsed;
                return;
            }

            _ramUsedStatic = ramUsed;
            var message = "Ram size " + ramUsed;
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
    }
}

internal class RamUsed
{
    private readonly long _ram1;
    private readonly long _ram2;

    public RamUsed(long ramUsed1, long ramUsed2)
    {
        _ram1 = ramUsed1;
        _ram2 = ramUsed2;
    }

    public override string ToString()
    {
        return _ram1 + " " + _ram2;
    }

    public bool InferioreDi(RamUsed ramUsed)
    {
        return _ram1 * 1.2d < ramUsed._ram1 || _ram2 * 1.2d < ramUsed._ram2;
    }
}