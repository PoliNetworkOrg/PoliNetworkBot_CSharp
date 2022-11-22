#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class RamSize
{
    private static RamUsedCollection? _ramUsedStatic;

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
            _ramUsedStatic ??= new RamUsedCollection();
            _ramUsedStatic.Append(ramUsed);

            if (!_ramUsedStatic.InferioreDi(ramUsed) && firstTime == false) return;

            var message = "#ramsize " + ramUsed;
            Logger.Logger.WriteLine(message);
            await SendMessage.SendMessageInAGroup(BotUtil.GetFirstModerationRealBot(), "en",
                new Language(
                    new Dictionary<string, string?>
                    {
                        { "en", message }
                    }), null, GroupsConstants.BackupGroup, ChatType.Group, ParseMode.Html, null, true);
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex, LogSeverityLevel.ERROR);
        }
    }
}

internal class RamUsedCollection
{
    private const int Limit = 100;
    private readonly List<RamUsed> _listRamUsed;

    public RamUsedCollection()
    {
        _listRamUsed = new List<RamUsed>();
    }

    public bool InferioreDi(RamUsed ramUsed)
    {
        return _listRamUsed.Count == 0 || _listRamUsed[^1].InferioreDi(ramUsed) || MediaInferiore(ramUsed);
    }

    private bool MediaInferiore(RamUsed ramUsed)
    {
        if (_listRamUsed.Count == 0)
            return true;

        var last = _listRamUsed[^1];
        var media = new RamUsed(last.Ram1, last.Ram2);
        for (var i = 0; i < _listRamUsed.Count - 1; i++)
        {
            media.Ram1 += _listRamUsed[i].Ram1;
            media.Ram2 += _listRamUsed[i].Ram2;
        }

        media.Ram1 /= _listRamUsed.Count;
        media.Ram2 /= _listRamUsed.Count;

        return media.InferioreDi(ramUsed);
    }

    public void Append(RamUsed ramUsed)
    {
        if (_listRamUsed.Count >= Limit) _listRamUsed.RemoveAt(0);

        _listRamUsed.Add(ramUsed);
    }
}

internal class RamUsed
{
    internal long Ram1;
    internal long Ram2;

    public RamUsed(long ramUsed1, long ramUsed2)
    {
        Ram1 = ramUsed1;
        Ram2 = ramUsed2;
    }

    public override string ToString()
    {
        return Ram1 + " " + Ram2;
    }

    public bool InferioreDi(RamUsed ramUsed)
    {
        return Ram1 * 1.2d < ramUsed.Ram1 || Ram2 * 1.2d < ramUsed.Ram2;
    }
}