using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Utils;

[Serializable]
internal class RamUsedCollection
{
    private const int Limit = 100;
    private readonly List<RamUsed> _listRamUsed = new();

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